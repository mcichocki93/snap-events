import { defineStore } from 'pinia'
import api from '../services/api'
import { getSafeFontFamily } from '../utils/fontUtils'
import { getEventTypeInfo } from '../utils/eventTypeUtils'
import type { Client, ThemeColors } from '../types'

interface ClientState {
  client: Client | null
  loading: boolean
  error: string | null
}

export const useClientStore = defineStore('client', {
  state: (): ClientState => ({
    client: null,
    loading: false,
    error: null
  }),

  getters: {
    isLoaded: (state): boolean => state.client !== null,
    
    canUpload: (state): boolean => {
      if (!state.client) return false
      return state.client.canUploadMore && 
             state.client.isActive && 
             !state.client.isExpired
    },
    
    themeColors: (state): ThemeColors => {
      if (!state.client) {
        return {
          background: '#667eea',
          backgroundSecondary: '#764ba2',
          font: '#ffffff',
          fontType: 'system-ui, -apple-system, sans-serif',
          accent: '#3b82f6'
        }
      }
      
      return {
        background: state.client.backgroundColor || '#667eea',
        backgroundSecondary: state.client.backgroundColorSecondary || '#764ba2',
        font: state.client.fontColor || '#ffffff',
        fontType: getSafeFontFamily(state.client.fontType),
        accent: state.client.accentColor || '#3b82f6'
      }
    }
  },

  actions: {
    async loadClient(guid: string): Promise<void> {
      this.loading = true
      this.error = null

      try {
        const clientData = await api.getClient(guid)

        // Enrich client data with event type info if not provided
        if (clientData.eventType && !clientData.eventTypeDisplayName) {
          const eventInfo = getEventTypeInfo(clientData.eventType)
          clientData.eventTypeDisplayName = eventInfo.displayName
          clientData.eventTypeEmoji = eventInfo.emoji
        }

        this.client = clientData
      } catch (error: any) {
        this.error = error.response?.data?.message || 'Nie udało się pobrać danych klienta'
        throw error
      } finally {
        this.loading = false
      }
    },

    async validateClient(guid: string): Promise<{ isValid: boolean; message: string; brideGroom?: string }> {
      return api.validateClient(guid)
    },

    updateUploadCount(): void {
      if (this.client) {
        this.client.uploadedFilesCount += 1
      }
    }
  }
})