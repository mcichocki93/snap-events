/**
 * Keeps an in-progress upload batch in the guest's own browser, so a phone
 * discarding the backgrounded tab does not lose the photos still waiting.
 *
 * IndexedDB rather than localStorage because only it can hold a File. That
 * matters more than the size limit: once the page reloads, the File handles
 * from the picker are gone for good and cannot be recreated without the guest
 * choosing the same photos again. Storing the bytes is the only way a reloaded
 * page can carry on.
 *
 * Everything here is best effort. Storage can be full, or blocked entirely in
 * private browsing - in which case uploading must still work exactly as it did
 * before, just without the safety net. No call in this module ever rejects.
 */

const DB_NAME = 'snapevents'
const DB_VERSION = 1
const STORE = 'uploadQueues'

// Older than this and it is a previous visit, not an interrupted send.
const MAX_AGE_MS = 24 * 60 * 60 * 1000

export interface StoredUploadItem {
  name: string
  file: File
  status: 'pending' | 'done' | 'failed'
  /** Set once Drive has a session open, so a resumed upload can continue it. */
  sessionUrl: string | null
  photoId: string | null
}

export interface StoredQueue {
  guid: string
  createdAt: number
  items: StoredUploadItem[]
}

function openDb(): Promise<IDBDatabase | null> {
  return new Promise(resolve => {
    if (typeof indexedDB === 'undefined') return resolve(null)

    try {
      const request = indexedDB.open(DB_NAME, DB_VERSION)

      request.onupgradeneeded = () => {
        const db = request.result
        if (!db.objectStoreNames.contains(STORE)) {
          db.createObjectStore(STORE, { keyPath: 'guid' })
        }
      }

      request.onsuccess = () => resolve(request.result)
      request.onerror = () => resolve(null)
      request.onblocked = () => resolve(null)
    } catch {
      resolve(null)
    }
  })
}

function runTransaction<T>(
  mode: IDBTransactionMode,
  work: (store: IDBObjectStore) => IDBRequest<T>
): Promise<T | null> {
  return new Promise(async resolve => {
    const db = await openDb()
    if (!db) return resolve(null)

    try {
      const tx = db.transaction(STORE, mode)
      const request = work(tx.objectStore(STORE))

      request.onsuccess = () => resolve(request.result ?? null)
      request.onerror = () => resolve(null)
      tx.onabort = () => resolve(null)
      tx.oncomplete = () => db.close()
    } catch {
      resolve(null)
    }
  })
}

export async function saveQueue(queue: StoredQueue): Promise<void> {
  await runTransaction('readwrite', store => store.put(queue))
}

/**
 * Returns an interrupted batch worth resuming, or null. Anything finished,
 * empty or stale is cleaned up rather than offered back to the guest.
 */
export async function loadQueue(guid: string): Promise<StoredQueue | null> {
  const queue = (await runTransaction<StoredQueue>('readonly', store =>
    store.get(guid)
  )) as StoredQueue | null

  if (!queue) return null

  if (Date.now() - queue.createdAt > MAX_AGE_MS) {
    await deleteQueue(guid)
    return null
  }

  const unfinished = queue.items.filter(item => item.status !== 'done')
  if (unfinished.length === 0) {
    await deleteQueue(guid)
    return null
  }

  return queue
}

export async function deleteQueue(guid: string): Promise<void> {
  await runTransaction('readwrite', store => store.delete(guid))
}
