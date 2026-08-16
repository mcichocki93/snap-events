import { ref, computed, type Ref, type ComputedRef } from 'vue'
import { useNotification } from './useNotification'
import api from '../services/api'
import type { PhotoInfo, ComposableResult } from '../types'

export interface UseGalleryReturn {
  photos: Ref<PhotoInfo[]>
  loading: Ref<boolean>
  loadingMore: Ref<boolean>
  error: Ref<string | null>
  hasMore: Ref<boolean>
  isEmpty: ComputedRef<boolean>
  photoCount: ComputedRef<number>
  totalCount: Ref<number>
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
  // Kept separate from loadingMore on purpose: the view swaps the whole grid for
  // a full-page spinner while this is true, which on "load more" tore the grid
  // down, threw the scroll position to the top and re-fetched every image.
  const loading = ref(false)
  const loadingMore = ref(false)
  const error = ref<string | null>(null)
  const currentPage = ref(1)
  const hasMore = ref(false)
  const totalCount = ref(0)

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

    if (reset) {
      loading.value = true
    } else {
      loadingMore.value = true
    }
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
      totalCount.value = response.totalCount

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
      loadingMore.value = false
    }
  }

  /**
   * Load more photos (pagination)
   */
  const loadMore = async (): Promise<void> => {
    if (!hasMore.value || loading.value || loadingMore.value) return

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

      // Create temporary link and trigger download.
      // Deliberately no target="_blank": the download attribute already hands
      // the transfer to the browser's download manager, which survives the
      // screen locking, whereas a new tab is a context mobile browsers discard.
      const link = document.createElement('a')
      link.href = proxyUrl
      link.download = photo.name
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
    loadingMore,
    error,
    hasMore,
    isEmpty,
    photoCount,
    totalCount,

    // Methods
    loadPhotos,
    loadMore,
    downloadPhoto
  }
}
