import { ref, computed, type Ref, type ComputedRef } from 'vue'
import { useNotification } from './useNotification'
import { useWakeLock } from './useWakeLock'
import { uploadFileResumable, DirectUploadUnavailableError } from './useResumableUpload'
import api from '../services/api'
import type { Client, FileUpload, ComposableResult } from '../types'

// Abort a file if no bytes move for this long. There is deliberately no overall
// timeout - a 20MB photo over a weak signal is slow but fine; a transfer that
// has stopped moving because the phone slept is not.
const STALL_TIMEOUT_MS = 60_000

// One initial try plus two retries. Kept low because /photo/upload is rate
// limited to 100 requests per hour per IP.
const MAX_ATTEMPTS = 3

export interface UploadBatchResult {
  uploaded: number
  failed: number
  at: number
}

export interface UsePhotoUploadReturn {
  selectedFiles: Ref<FileUpload[]>
  uploading: Ref<boolean>
  uploadedCount: Ref<number>
  lastBatch: Ref<UploadBatchResult | null>
  hasFiles: ComputedRef<boolean>
  canUpload: ComputedRef<boolean>
  addFiles: (files: FileList | File[]) => void
  removeFile: (index: number) => void
  uploadFiles: () => Promise<ComposableResult<{ count: number }>>
  startNewBatch: () => void
  clearFiles: () => void
  formatFileSize: (bytes: number) => string
}

/**
 * Composable for handling photo uploads
 * @param guid - Client GUID
 * @param clientRef - Ref to client data object
 */
export function usePhotoUpload(guid: string, clientRef: Ref<Client | null>): UsePhotoUploadReturn {
  const { notify } = useNotification()
  const wakeLock = useWakeLock()

  const selectedFiles = ref<FileUpload[]>([])
  const uploading = ref(false)
  const uploadedCount = ref(0)

  // Flipped off for the rest of the session the first time the direct-to-Drive
  // route proves unreachable, so we do not pay that discovery cost per photo.
  let directUploadAvailable = true

  // Result of the most recent batch, kept on screen until the guest dismisses
  // it. Mirrored into sessionStorage so it survives the page being reloaded or
  // discarded while backgrounded - the exact case where the guest most needs
  // telling that their photos arrived.
  const lastBatch = ref<UploadBatchResult | null>(null)
  const batchStorageKey = `snapevents:lastUpload:${guid}`

  const recordBatch = (uploaded: number, failed: number): void => {
    const result: UploadBatchResult = { uploaded, failed, at: Date.now() }
    lastBatch.value = result

    try {
      sessionStorage.setItem(batchStorageKey, JSON.stringify(result))
    } catch {
      // Storage unavailable (private mode); the on-screen summary still works.
    }
  }

  const restoreLastBatch = (): void => {
    try {
      const stored = sessionStorage.getItem(batchStorageKey)
      if (!stored) return

      const result = JSON.parse(stored) as UploadBatchResult
      // Anything older than an hour is a previous visit, not this one.
      if (Date.now() - result.at < 60 * 60 * 1000) lastBatch.value = result
    } catch {
      // Corrupt or unavailable; start without a summary.
    }
  }

  restoreLastBatch()

  /**
   * A rejection is worth retrying only when the server never made a decision.
   * Anything it answered - wrong file type, quota exhausted, rate limited -
   * would be rejected identically next time.
   */
  const isRetryable = (error: any): boolean => {
    const status = error?.response?.status
    if (status === undefined) return true // network error, abort, or stall
    return status >= 500
  }

  /**
   * Waits until the page is back in the foreground and the device is online.
   * This is what makes a locked phone recoverable: the retry parks here until
   * the guest unlocks, then carries on instead of failing while nothing can
   * possibly succeed.
   */
  const waitUntilConnected = (): Promise<void> => {
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

  /**
   * Uploads one file, preferring the resumable path straight to Drive so an
   * interruption costs only the current chunk. Falls back to the buffered
   * endpoint when that route is unavailable - an old browser, or a network that
   * blocks Google's upload host.
   */
  const uploadOne = async (fileObj: FileUpload): Promise<void> => {
    if (directUploadAvailable) {
      try {
        await uploadFileResumable(guid, fileObj.file, (percent) => {
          fileObj.progress = percent
        })
        return
      } catch (error) {
        if (!(error instanceof DirectUploadUnavailableError)) throw error

        // Stop trying for the rest of the batch; one failure to reach the
        // session route means the others will not fare better.
        directUploadAvailable = false
        fileObj.progress = 0
      }
    }

    await uploadViaServer(fileObj)
  }

  /**
   * The original path: one whole multipart POST through our API. Kept as a
   * fallback, and still guarded against stalls.
   */
  const uploadViaServer = async (fileObj: FileUpload): Promise<void> => {
    const controller = new AbortController()
    let stallTimer: ReturnType<typeof setTimeout>

    const restartStallTimer = () => {
      clearTimeout(stallTimer)
      stallTimer = setTimeout(() => controller.abort(), STALL_TIMEOUT_MS)
    }

    restartStallTimer()

    try {
      await api.uploadPhoto(
        guid,
        fileObj.file,
        (progressEvent) => {
          restartStallTimer()

          if (progressEvent.total) {
            fileObj.progress = Math.round(
              (progressEvent.loaded * 100) / progressEvent.total
            )
          }
        },
        controller.signal
      )
    } finally {
      clearTimeout(stallTimer!)
    }
  }

  /**
   * Format file size to human readable format
   */
  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 B'
    const k = 1024
    const sizes = ['B', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
  }

  // Allowed image MIME types (must match backend AllowedMimeTypes)
  const ALLOWED_MIME_TYPES = new Set([
    'image/jpeg',
    'image/jpg',
    'image/png',
    'image/gif',
    'image/webp',
    'image/heic',
    'image/heif'
  ])

  /**
   * Validate file before adding to selection
   */
  const validateFile = (file: File): string[] => {
    const errors: string[] = []

    if (!clientRef.value) return ['Brak danych klienta']

    if (file.size > clientRef.value.maxFileSize) {
      const maxSizeMB = Math.round(clientRef.value.maxFileSize / (1024 * 1024))
      errors.push(`Plik ${file.name} jest za duży (max ${maxSizeMB}MB)`)
    }

    if (!ALLOWED_MIME_TYPES.has(file.type.toLowerCase())) {
      errors.push(`Plik ${file.name} nie jest obsługiwanym typem zdjęcia (dozwolone: JPG, PNG, GIF, WEBP, HEIC)`)
    }

    return errors
  }

  /**
   * Add files to selection
   */
  const addFiles = (files: FileList | File[]): void => {
    if (!clientRef.value) return

    const fileArray = Array.from(files)
    const validFiles: FileUpload[] = []

    for (const file of fileArray) {
      const errors = validateFile(file)

      if (errors.length > 0) {
        errors.forEach(error => {
          notify({
            type: 'warning',
            message: error
          })
        })
        continue
      }

      validFiles.push({
        file: file,
        name: file.name,
        size: file.size,
        uploading: false,
        uploaded: false,
        error: false,
        progress: 0
      })
    }

    selectedFiles.value.push(...validFiles)

    // Check if too many files selected, accounting for already uploaded files
    const maxFiles = clientRef.value.maxFiles
    const alreadyUploaded = clientRef.value.uploadedFilesCount
    const remaining = maxFiles > 0 ? maxFiles - alreadyUploaded : Infinity
    if (maxFiles > 0 && selectedFiles.value.length > remaining) {
      notify({
        type: 'warning',
        message: `Możesz przesłać jeszcze maksymalnie ${remaining} zdjęć`
      })
      selectedFiles.value = selectedFiles.value.slice(0, remaining)
    }
  }

  /**
   * Remove file from selection
   */
  const removeFile = (index: number): void => {
    selectedFiles.value.splice(index, 1)
  }

  /**
   * Upload all selected files
   */
  const uploadFiles = async (): Promise<ComposableResult<{ count: number }>> => {
    if (selectedFiles.value.length === 0) {
      return { success: false, message: 'Brak plików do przesłania' }
    }

    if (!clientRef.value) {
      return { success: false, message: 'Brak danych klienta' }
    }

    if (clientRef.value.maxFiles > 0 && selectedFiles.value.length > clientRef.value.maxFiles) {
      return { success: false, message: 'Za dużo plików' }
    }

    uploading.value = true
    uploadedCount.value = 0

    // Hold the screen awake for the whole batch, not per file, so the phone
    // cannot lock in the gap between two photos.
    await wakeLock.acquire()

    try {
      for (let i = 0; i < selectedFiles.value.length; i++) {
        const fileObj = selectedFiles.value[i]

        if (fileObj.uploaded) continue

        fileObj.uploading = true
        fileObj.error = false

        for (let attempt = 1; attempt <= MAX_ATTEMPTS; attempt++) {
          try {
            await uploadOne(fileObj)

            fileObj.uploaded = true
            fileObj.uploading = false
            uploadedCount.value++

            notify({
              type: 'positive',
              message: `Przesłano: ${fileObj.name}`
            })
            break
          } catch (error) {
            const canRetry = attempt < MAX_ATTEMPTS && isRetryable(error)

            if (!canRetry) {
              fileObj.error = true
              fileObj.uploading = false
              fileObj.progress = 0

              notify({
                type: 'negative',
                message: `Błąd przesyłania: ${fileObj.name}`
              })
              break
            }

            // Park until there is a working connection again, then start this
            // file over. Restarting is the only option - the API takes a photo
            // as one whole request, so a partial transfer cannot be resumed.
            fileObj.progress = 0
            await waitUntilConnected()
            await wakeLock.acquire()
          }
        }
      }
    } finally {
      uploading.value = false
      await wakeLock.release()
    }

    // Deliberately not cleared on a timer, and deliberately not a toast. A
    // batch often finishes while the guest is looking at something else, and
    // the old timed toast plus the two-second list wipe meant they came back to
    // a blank screen and concluded nothing had been sent.
    recordBatch(uploadedCount.value, selectedFiles.value.filter(f => f.error).length)

    if (uploadedCount.value > 0) {
      return { success: true, data: { count: uploadedCount.value } }
    }

    return { success: false, message: 'Nie udało się przesłać żadnego pliku' }
  }

  /**
   * Drops the files already sent and the summary, leaving a clean screen for
   * the next batch. Called when the guest chooses to send more.
   */
  const startNewBatch = (): void => {
    selectedFiles.value = selectedFiles.value.filter(f => !f.uploaded)
    uploadedCount.value = 0
    lastBatch.value = null

    try {
      sessionStorage.removeItem(batchStorageKey)
    } catch {
      // Storage unavailable (private mode); the in-memory state is enough.
    }
  }

  /**
   * Clear all selected files
   */
  const clearFiles = (): void => {
    selectedFiles.value = []
    uploadedCount.value = 0
  }

  // Computed
  const hasFiles = computed(() => selectedFiles.value.length > 0)
  const canUpload = computed(() => {
    if (!hasFiles.value || clientRef.value === null || uploading.value) return false
    const { maxFiles, uploadedFilesCount } = clientRef.value
    if (maxFiles === 0) return true // unlimited
    const remaining = maxFiles - uploadedFilesCount
    return remaining > 0 && selectedFiles.value.length <= remaining
  })

  return {
    // State
    selectedFiles,
    uploading,
    uploadedCount,
    lastBatch,
    hasFiles,
    canUpload,

    // Methods
    addFiles,
    removeFile,
    uploadFiles,
    startNewBatch,
    clearFiles,
    formatFileSize
  }
}
