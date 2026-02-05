import { ref, computed, type Ref, type ComputedRef } from 'vue'
import { useNotification } from './useNotification'
import { useClientStore } from '../stores/clientStore'
import type { Client, ThemeColors, ComposableResult } from '../types'

export interface UseClientDataReturn {
  loading: Ref<boolean>
  error: Ref<string | null>
  isInactive: Ref<boolean>
  client: ComputedRef<Client | null>
  isLoaded: ComputedRef<boolean>
  isExpired: ComputedRef<boolean>
  isActive: ComputedRef<boolean>
  canUpload: ComputedRef<boolean>
  themeColors: ComputedRef<ThemeColors>
  loadClient: () => Promise<ComposableResult>
  showExpiredWarning: () => void
}

/**
 * Composable for loading and managing client data
 * @param guid - Client GUID
 */
export function useClientData(guid: string): UseClientDataReturn {
  const { notify } = useNotification()
  const clientStore = useClientStore()

  const loading = ref(false)
  const error = ref<string | null>(null)
  const isInactive = ref(false)

  // Computed properties
  const client = computed(() => clientStore.client)
  const isLoaded = computed(() => clientStore.isLoaded)
  const isExpired = computed(() => client.value?.isExpired || false)
  const isActive = computed(() => client.value?.isActive || false)
  const themeColors = computed(() => clientStore.themeColors)

  /**
   * Load client data from API
   */
  const loadClient = async (): Promise<ComposableResult> => {
    if (clientStore.isLoaded) {
      return { success: true }
    }

    loading.value = true
    error.value = null
    isInactive.value = false

    try {
      await clientStore.loadClient(guid)
      return { success: true }
    } catch (err: any) {
      const status = err.response?.status
      const message = err.response?.data?.message || 'Nie udało się załadować danych klienta'

      error.value = message

      // Handle different error types
      if (status === 400) {
        isInactive.value = true
        notify({
          type: 'negative',
          message: message,
          timeout: 5000
        })
      } else {
        notify({
          type: 'negative',
          message: message
        })
      }

      return { success: false, status, message }
    } finally {
      loading.value = false
    }
  }

  /**
   * Check if client can upload photos
   */
  const canUpload = computed(() => {
    return isActive.value && !isExpired.value
  })

  /**
   * Show expired warning notification
   */
  const showExpiredWarning = (): void => {
    if (isExpired.value) {
      notify({
        type: 'warning',
        message: 'Link do galerii wygasł - możesz przeglądać zdjęcia, ale nie dodawać nowych',
        timeout: 5000
      })
    }
  }

  return {
    // State
    loading,
    error,
    isInactive,
    client,
    isLoaded,
    isExpired,
    isActive,
    canUpload,
    themeColors,

    // Methods
    loadClient,
    showExpiredWarning
  }
}
