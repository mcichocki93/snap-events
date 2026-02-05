/**
 * Domain types for WeddingPhotos application
 */

export type EventType = 'Wedding' | 'Birthday' | 'Baptism' | 'Communion' | 'Corporate' | 'Conference' | 'Other'

export interface Client {
  guid: string
  eventName: string // Replaces brideGroom
  eventType: EventType
  eventTypeDisplayName?: string // Computed from eventType if not provided
  eventTypeEmoji?: string // Computed from eventType if not provided
  eventDate: string | null // ISO date string
  dateTo: string // ISO date string
  isActive: boolean
  isExpired: boolean
  maxFiles: number
  uploadedFilesCount: number
  canUploadMore: boolean
  maxFileSize: number
  backgroundColor: string
  backgroundColorSecondary: string
  fontColor: string
  fontType: string
  accentColor: string
}

export interface ThemeColors {
  background: string
  backgroundSecondary: string
  font: string
  fontType: string
  accent: string
}

export interface PhotoInfo {
  id: string
  name: string
  thumbnailUrl: string
  fullUrl: string
  dateAdded: string // ISO date string
  size: number
  mimeType: string
  formattedSize: string
  fileExtension: string
  isNew: boolean
}

export interface GalleryResponse {
  photos: PhotoInfo[]
  totalCount: number
  hasMore: boolean
  nextPageToken: string | null
}

export interface UploadPhotoResponse {
  success: boolean
  photoId?: string
  message: string
  remainingUploads?: number
}

export interface FileUpload {
  file: File
  name: string
  size: number
  uploading: boolean
  uploaded: boolean
  error: boolean
  progress: number
}

export interface ApiError {
  message: string
  status?: number
}

export interface ComposableResult<T = void> {
  success: boolean
  data?: T
  message?: string
  status?: number
}
