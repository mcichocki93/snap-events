import { ref, computed, type Ref, type ComputedRef } from 'vue'
import { useNotification } from './useNotification'
import api from '../services/api'
import type { PhotoInfo, ComposableResult } from '../types'

export interface UseGalleryReturn {
  photos: Ref<PhotoInfo[]>
  loading: Ref<boolean>
  error: Ref<string | null>
  hasMore: Ref<boolean>
  isEmpty: ComputedRef<boolean>
  photoCount: ComputedRef<number>
  loadPhotos: (reset?: boolean) => Promise<ComposableResult>
  loadMore: () => Promise<void>
  downloadPhoto: (photo: PhotoInfo) => Promise<ComposableResult>
}

/**
 * Composable for loading and managing gallery photos
 * @param guid - Client GUID
 */
export function useGallery(guid: string): UseGalleryReturn {
  const { notify } = useNotification()

  const photos = ref<PhotoInfo[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const currentPage = ref(1)
  const hasMore = ref(false)

  /**
   * Map photo with proxy URLs to avoid tracking prevention
   */
  const mapPhotoWithProxyUrls = (photo: PhotoInfo): PhotoInfo => ({
    ...photo,
    thumbnailUrl: api.getProxyThumbnailUrl(photo.id),
    fullUrl: api.getProxyFullUrl(photo.id)
  })

  /**
   * Load photos from gallery
   */
  const loadPhotos = async (reset: boolean = false): Promise<ComposableResult> => {
    if (reset) {
      currentPage.value = 1
      photos.value = []
    }

    loading.value = true
    error.value = null

    try {
      const response = await api.getGallery(guid, currentPage.value)

      // Map photos with proxy URLs
      const proxiedPhotos = response.photos.map(mapPhotoWithProxyUrls)

      if (reset) {
        photos.value = proxiedPhotos
      } else {
        photos.value.push(...proxiedPhotos)
      }

      hasMore.value = response.hasMore

      return { success: true }
    } catch (err: any) {
      const message = err.response?.data?.message || 'Nie udało się załadować galerii'
      error.value = message

      notify({
        type: 'negative',
        message: message
      })

      return { success: false, message }
    } finally {
      loading.value = false
    }
  }

  /**
   * Load more photos (pagination)
   */
  const loadMore = async (): Promise<void> => {
    if (!hasMore.value || loading.value) return

    currentPage.value++

    const result = await loadPhotos(false)

    if (!result.success) {
      currentPage.value-- // Revert page counter on failure
    }
  }

  /**
   * Download a photo
   */
  const downloadPhoto = async (photo: PhotoInfo): Promise<ComposableResult> => {
    try {
      const proxyUrl = api.getProxyDownloadUrl(photo.id)

      // Create temporary link and trigger download
      const link = document.createElement('a')
      link.href = proxyUrl
      link.download = photo.name
      link.target = '_blank'
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)

      notify({
        type: 'positive',
        message: 'Pobieranie rozpoczęte'
      })

      return { success: true }
    } catch (err) {
      notify({
        type: 'negative',
        message: 'Nie udało się pobrać zdjęcia'
      })

      return { success: false }
    }
  }

  // Computed
  const isEmpty = computed(() => !loading.value && photos.value.length === 0)
  const photoCount = computed(() => photos.value.length)

  return {
    // State
    photos,
    loading,
    error,
    hasMore,
    isEmpty,
    photoCount,

    // Methods
    loadPhotos,
    loadMore,
    downloadPhoto
  }
}
