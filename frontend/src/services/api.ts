import axios, { AxiosProgressEvent } from 'axios'
import type { Client, GalleryResponse, UploadPhotoResponse } from '../types'

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api'
const isDev = import.meta.env.DEV

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor for logging (only in development)
apiClient.interceptors.request.use(
  (config) => {
    if (isDev) {
      console.log(`API Request: ${config.method?.toUpperCase()} ${config.url}`)
    }
    return config
  },
  (error) => {
    if (isDev) {
      console.error('API Request Error:', error)
    }
    return Promise.reject(error)
  }
)

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    if (isDev) {
      console.error('API Response Error:', error.response?.data || error.message)
    }
    return Promise.reject(error)
  }
)

export default {
  /**
   * Get client data by GUID
   */
  async getClient(guid: string): Promise<Client> {
    const response = await apiClient.get<Client>(`/client/${guid}`)
    return response.data
  },

  /**
   * Validate client access
   */
  async validateClient(guid: string): Promise<{ isValid: boolean; message: string; brideGroom?: string }> {
    const response = await apiClient.get(`/client/${guid}/validate`)
    return response.data
  },

  /**
   * Get gallery photos
   */
  async getGallery(guid: string, page: number = 1, pageSize: number = 50): Promise<GalleryResponse> {
    const response = await apiClient.get<GalleryResponse>(`/photo/gallery/${guid}`, {
      params: { page, pageSize }
    })
    return response.data
  },

  /**
   * Upload a photo
   */
  async uploadPhoto(
    guid: string, 
    file: File, 
    onProgress?: (progress: AxiosProgressEvent) => void
  ): Promise<UploadPhotoResponse> {
    const formData = new FormData()
    formData.append('file', file)

    const response = await apiClient.post<UploadPhotoResponse>(`/photo/upload/${guid}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      },
      onUploadProgress: onProgress
    })

    return response.data
  },

  /**
   * Get download URL for a photo
   */
  async getDownloadUrl(photoId: string): Promise<{ downloadUrl: string }> {
    const response = await apiClient.get<{ downloadUrl: string }>(`/photo/download/${photoId}`)
    return response.data
  },

  /**
   * Get proxied thumbnail URL (avoids tracking prevention)
   */
  getProxyThumbnailUrl(photoId: string): string {
    return `${API_BASE_URL}/photo/proxy/${photoId}?size=thumb`
  },

  /**
   * Get proxied full image URL (avoids tracking prevention)
   */
  getProxyFullUrl(photoId: string): string {
    return `${API_BASE_URL}/photo/proxy/${photoId}`
  },

  /**
   * Get proxied download URL (avoids tracking prevention)
   */
  getProxyDownloadUrl(photoId: string): string {
    return `${API_BASE_URL}/photo/proxy/${photoId}/download`
  },

  /**
   * Send contact form message
   */
  async sendContactMessage(data: {
    name: string
    email: string
    phone?: string
    message: string
  }): Promise<{ success: boolean; message: string }> {
    const response = await apiClient.post<{ success: boolean; message: string }>('/contact', data)
    return response.data
  }
}