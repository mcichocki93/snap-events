export interface AdminClient {
  guid: string
  firstName: string
  lastName: string
  email: string
  phone?: string
  eventName: string
  eventType: string
  eventTypeDisplayName: string
  eventTypeEmoji: string
  eventDate?: string
  dateTo: string
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
  googleStorageUrl: string
  createdAt: string
  updatedAt: string
}

export interface CreateClientPayload {
  guid: string
  firstName: string
  lastName: string
  email: string
  phone?: string
  eventName: string
  eventType: string
  eventDate?: string
  dateTo: string
  maxFiles: number
  maxFileSize: number
  backgroundColor?: string
  backgroundColorSecondary?: string
  fontColor?: string
  fontType?: string
  accentColor?: string
  googleStorageUrl: string
}

export interface UpdateClientPayload {
  firstName?: string
  lastName?: string
  email?: string
  phone?: string
  eventName?: string
  eventType?: string
  eventDate?: string
  dateTo?: string
  isActive?: boolean
  maxFiles?: number
  maxFileSize?: number
  backgroundColor?: string
  backgroundColorSecondary?: string
  fontColor?: string
  fontType?: string
  accentColor?: string
  googleStorageUrl?: string
}
