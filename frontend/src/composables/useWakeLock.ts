import { ref, onUnmounted, type Ref } from 'vue'

interface WakeLockSentinelLike {
  released: boolean
  release: () => Promise<void>
  addEventListener: (type: 'release', listener: () => void) => void
}

export interface UseWakeLockReturn {
  active: Ref<boolean>
  isSupported: boolean
  acquire: () => Promise<void>
  release: () => Promise<void>
}

/**
 * Keeps the screen awake for the duration of a long task.
 *
 * Phones dim and lock the screen after a short idle timeout, and a locked screen
 * suspends the radio, which killed in-progress uploads. Holding a screen wake
 * lock stops the automatic lock while photos are going out.
 *
 * The browser revokes the lock every time the page is hidden and will not let it
 * be re-taken until the page is visible again, so we re-acquire on the way back.
 */
export function useWakeLock(): UseWakeLockReturn {
  const active = ref(false)
  const isSupported = typeof navigator !== 'undefined' && 'wakeLock' in navigator

  let sentinel: WakeLockSentinelLike | null = null
  // Whether the caller still wants the lock, independent of whether we hold it.
  let wanted = false

  const acquire = async (): Promise<void> => {
    wanted = true

    if (!isSupported || sentinel || document.visibilityState !== 'visible') return

    try {
      sentinel = await (navigator as any).wakeLock.request('screen')
      active.value = true

      sentinel!.addEventListener('release', () => {
        sentinel = null
        active.value = false
      })
    } catch {
      // Denied - typically battery saver or a background tab. Uploads still run;
      // the screen just is not held awake, so this is not worth surfacing.
      active.value = false
    }
  }

  const release = async (): Promise<void> => {
    wanted = false

    if (!sentinel) return

    try {
      await sentinel.release()
    } catch {
      // Already gone; the release listener has done the bookkeeping.
    }

    sentinel = null
    active.value = false
  }

  const handleVisibilityChange = () => {
    if (wanted && document.visibilityState === 'visible') {
      void acquire()
    }
  }

  if (typeof document !== 'undefined') {
    document.addEventListener('visibilitychange', handleVisibilityChange)
  }

  onUnmounted(() => {
    document.removeEventListener('visibilitychange', handleVisibilityChange)
    void release()
  })

  return { active, isSupported, acquire, release }
}
