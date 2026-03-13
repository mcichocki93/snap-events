import { ref, computed, type Ref, type ComputedRef } from 'vue'
import { useNotification } from './useNotification'
import api from '../services/api'
import type { Client, FileUpload, ComposableResult } from '../types'

export interface UsePhotoUploadReturn {
  selectedFiles: Ref<FileUpload[]>
  uploading: Ref<boolean>
  uploadedCount: Ref<number>
  hasFiles: ComputedRef<boolean>
  canUpload: ComputedRef<boolean>
  addFiles: (files: FileList | File[]) => void
  removeFile: (index: number) => void
  uploadFiles: () => Promise<ComposableResult<{ count: number }>>
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

  const selectedFiles = ref<FileUpload[]>([])
  const uploading = ref(false)
  const uploadedCount = ref(0)

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

    if (!file.type.startsWith('image/')) {
      errors.push(`Plik ${file.name} nie jest zdjęciem`)
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

    // Check if too many files selected (maxFiles === 0 means unlimited)
    const maxFiles = clientRef.value.maxFiles
    if (maxFiles > 0 && selectedFiles.value.length > maxFiles) {
      notify({
        type: 'warning',
        message: `Możesz przesłać maksymalnie ${maxFiles} zdjęć na raz`
      })
      selectedFiles.value = selectedFiles.value.slice(0, maxFiles)
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

    for (let i = 0; i < selectedFiles.value.length; i++) {
      const fileObj = selectedFiles.value[i]

      if (fileObj.uploaded) continue

      fileObj.uploading = true
      fileObj.error = false

      try {
        await api.uploadPhoto(
          guid,
          fileObj.file,
          (progressEvent) => {
            if (progressEvent.total) {
              fileObj.progress = Math.round(
                (progressEvent.loaded * 100) / progressEvent.total
              )
            }
          }
        )

        fileObj.uploaded = true
        fileObj.uploading = false
        uploadedCount.value++

        notify({
          type: 'positive',
          message: `Przesłano: ${fileObj.name}`
        })
      } catch (error) {
        fileObj.error = true
        fileObj.uploading = false

        notify({
          type: 'negative',
          message: `Błąd przesyłania: ${fileObj.name}`
        })
      }
    }

    uploading.value = false

    if (uploadedCount.value > 0) {
      notify({
        type: 'positive',
        message: `Pomyślnie przesłano ${uploadedCount.value} zdjęć!`,
        timeout: 5000
      })

      // Clear uploaded files after delay
      setTimeout(() => {
        selectedFiles.value = selectedFiles.value.filter(f => !f.uploaded)
      }, 2000)

      return { success: true, data: { count: uploadedCount.value } }
    }

    return { success: false, message: 'Nie udało się przesłać żadnego pliku' }
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
    return hasFiles.value &&
           clientRef.value !== null &&
           (clientRef.value.maxFiles === 0 || selectedFiles.value.length <= clientRef.value.maxFiles) &&
           !uploading.value
  })

  return {
    // State
    selectedFiles,
    uploading,
    uploadedCount,
    hasFiles,
    canUpload,

    // Methods
    addFiles,
    removeFile,
    uploadFiles,
    clearFiles,
    formatFileSize
  }
}
