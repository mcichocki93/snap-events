import api from '../services/api'

// Drive requires every chunk except the last to be a multiple of 256KB.
const CHUNK_SIZE = 8 * 256 * 1024 // 2MB

// Abort a chunk that stops moving. Chunks are small, so this can be tighter
// than a whole-file timeout would ever be.
const CHUNK_STALL_TIMEOUT_MS = 45_000

const MAX_CHUNK_ATTEMPTS = 5

const MAX_SESSION_ATTEMPTS = 4

export class DirectUploadUnavailableError extends Error {}

/**
 * Waits for the page to be in the foreground with the device online.
 *
 * This is what makes a locked phone recoverable: the upload parks here rather
 * than burning attempts while nothing can succeed, and picks up the moment the
 * guest comes back.
 */
function waitUntilConnected(): Promise<void> {
  if (navigator.onLine !== false && document.visibilityState === 'visible') {
    return Promise.resolve()
  }

  return new Promise(resolve => {
    const check = () => {
      if (navigator.onLine !== false && document.visibilityState === 'visible') {
        window.removeEventListener('online', check)
        document.removeEventListener('visibilitychange', check)
        resolve()
      }
    }

    window.addEventListener('online', check)
    document.addEventListener('visibilitychange', check)
  })
}

function delay(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms))
}

let disarmUnloadRelease: (() => void) | null = null

/**
 * While a session is open, arrange for its quota slot to be handed back if the
 * page is destroyed mid-upload.
 *
 * A phone discarding a backgrounded tab kills any in-flight request, so the
 * ordinary cancel call never leaves the device and the gallery silently loses a
 * photo from its allowance. sendBeacon is the one request that survives.
 */
function armUnloadRelease(guid: string): void {
  releaseUnloadHandler()

  const handler = (event: PageTransitionEvent) => {
    // persisted means the page is only going into the back/forward cache and
    // may well come back to finish the upload.
    if (event.persisted) return
    navigator.sendBeacon?.(api.getCancelUploadUrl(guid))
  }

  window.addEventListener('pagehide', handler)
  disarmUnloadRelease = () => window.removeEventListener('pagehide', handler)
}

function releaseUnloadHandler(): void {
  disarmUnloadRelease?.()
  disarmUnloadRelease = null
}

/**
 * Sends one request with a stall guard, resolving to the raw XHR.
 *
 * XMLHttpRequest rather than fetch because only it reports upload progress,
 * which both drives the progress bar and tells us the transfer is alive.
 */
function sendChunk(
  url: string,
  blob: Blob,
  contentRange: string,
  onProgress: (bytesSent: number) => void
): Promise<XMLHttpRequest> {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest()
    let stallTimer: ReturnType<typeof setTimeout>

    const restartStallTimer = () => {
      clearTimeout(stallTimer)
      stallTimer = setTimeout(() => xhr.abort(), CHUNK_STALL_TIMEOUT_MS)
    }

    xhr.open('PUT', url, true)
    xhr.setRequestHeader('Content-Range', contentRange)

    xhr.upload.onprogress = (event) => {
      restartStallTimer()
      onProgress(event.loaded)
    }

    // Drive answers 308 for "chunk stored, send the next one". XHR surfaces that
    // as a normal response, so anything that arrives is a result, not a failure.
    xhr.onload = () => {
      clearTimeout(stallTimer)
      resolve(xhr)
    }

    xhr.onerror = () => {
      clearTimeout(stallTimer)
      reject(new Error('Network error while uploading chunk'))
    }

    xhr.onabort = () => {
      clearTimeout(stallTimer)
      reject(new Error('Chunk upload stalled'))
    }

    restartStallTimer()
    xhr.send(blob)
  })
}

/**
 * Asks Drive how much of the file it already holds, so an interrupted upload
 * resumes instead of starting over.
 *
 * Returns the next byte offset Drive expects, or -1 when the file is already
 * complete.
 */
async function queryReceivedBytes(url: string, totalSize: number): Promise<number> {
  const xhr = await sendChunk(url, new Blob([]), `bytes */${totalSize}`, () => {})

  if (xhr.status === 200 || xhr.status === 201) return -1

  if (xhr.status === 308) {
    const range = xhr.getResponseHeader('Range')
    if (!range) return 0

    // "bytes=0-262143" means Drive holds through byte 262143, so resume at the
    // one after it.
    const lastByte = Number(range.split('-')[1])
    return Number.isFinite(lastByte) ? lastByte + 1 : 0
  }

  throw new Error(`Cannot resume upload: Drive answered ${xhr.status}`)
}

/**
 * Opens an upload session, distinguishing "this route does not work here" from
 * "we currently have no network".
 *
 * Getting this wrong is expensive: treating a backgrounded phone's failed
 * request as proof the route is unsupported permanently drops the whole batch
 * onto the buffered endpoint, which cannot resume - so one moment offline used
 * to cost every remaining photo.
 */
async function openSession(
  guid: string,
  file: File
): Promise<{ uploadUrl?: string | null }> {
  for (let attempt = 1; ; attempt++) {
    try {
      return await api.createUploadSession(guid, file)
    } catch (error: any) {
      const status = error?.response?.status

      // The server answered and made a decision - quota spent, gallery
      // expired, file rejected. Surface it; retrying changes nothing.
      if (error?.response?.data?.message) throw error

      // The route genuinely is not there. This is the real fallback case.
      if (status === 404 || status === 405 || status === 501) {
        throw new DirectUploadUnavailableError(`Upload session endpoint answered ${status}`)
      }

      if (attempt >= MAX_SESSION_ATTEMPTS) {
        throw new Error('Could not open an upload session')
      }

      // No response at all: almost always a sleeping radio or a backgrounded
      // page. Wait for the page to come back and try again.
      await waitUntilConnected()
      await delay(Math.min(1000 * 2 ** (attempt - 1), 4000))
    }
  }
}

/**
 * Uploads a file straight to Drive in chunks, resuming across interruptions.
 *
 * @returns the new Drive file ID
 */
export async function uploadFileResumable(
  guid: string,
  file: File,
  onProgress: (percent: number) => void
): Promise<string> {
  const session = await openSession(guid, file)

  const uploadUrl = session.uploadUrl
  if (!uploadUrl) throw new DirectUploadUnavailableError('No upload session URL returned')

  // The session now holds a quota slot; make sure it comes back even if the
  // page is destroyed before this finishes.
  armUnloadRelease(guid)

  try {
    return await sendChunks(guid, file, uploadUrl, onProgress)
  } finally {
    releaseUnloadHandler()
  }
}

async function sendChunks(
  guid: string,
  file: File,
  uploadUrl: string,
  onProgress: (percent: number) => void
): Promise<string> {
  let offset = 0
  let attempts = 0

  while (offset < file.size) {
    const end = Math.min(offset + CHUNK_SIZE, file.size)
    const chunk = file.slice(offset, end)
    const contentRange = `bytes ${offset}-${end - 1}/${file.size}`

    try {
      const xhr = await sendChunk(uploadUrl, chunk, contentRange, (bytesSent) => {
        onProgress(Math.round(((offset + bytesSent) * 100) / file.size))
      })

      if (xhr.status === 200 || xhr.status === 201) {
        onProgress(100)
        const photoId = JSON.parse(xhr.responseText)?.id
        if (!photoId) throw new Error('Drive accepted the upload but returned no file ID')

        await api.completeUploadSession(guid, photoId)
        return photoId
      }

      if (xhr.status === 308) {
        offset = end
        attempts = 0
        continue
      }

      // 4xx here is terminal: an expired or already-finished session.
      throw new Error(`Drive rejected a chunk with ${xhr.status}`)
    } catch (error) {
      attempts++

      if (attempts >= MAX_CHUNK_ATTEMPTS) {
        // Hand the quota slot back before giving up, so an abandoned upload
        // does not permanently cost the gallery one photo.
        await api.cancelUploadSession(guid).catch(() => {})
        throw error
      }

      await waitUntilConnected()
      await delay(Math.min(1000 * 2 ** (attempts - 1), 8000))

      // Re-ask Drive where it got to; a chunk may well have landed even though
      // the response never came back.
      try {
        const received = await queryReceivedBytes(uploadUrl, file.size)
        if (received === -1) {
          onProgress(100)
          throw new Error('Upload already complete but file ID unknown')
        }
        offset = received
      } catch {
        // Leave the offset alone and retry the same chunk. Drive ignores bytes
        // it already holds, so repeating one is safe.
      }
    }
  }

  throw new Error('Upload finished without Drive confirming the file')
}
