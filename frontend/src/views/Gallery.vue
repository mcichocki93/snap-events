<template>
  <div class="gallery-page" :style="pageStyle">
    <div class="container">
      <!-- Header -->
      <GalleryHeader
        :event-name="client?.eventName"
        :event-type-display="client?.eventTypeDisplayName"
        :event-emoji="client?.eventTypeEmoji"
        :event-date="client?.eventDate"
        :theme-colors="themeColors"
      />

      <!-- Inactive/Expired Banners -->
      <GalleryStatusBanners
        :is-expired="isExpired"
        :is-inactive="isInactive"
      />

      <!-- Action Buttons -->
      <GalleryActions
        :can-upload="canUpload"
        :is-expired="isExpired"
        :is-active="isActive"
        :theme-colors="themeColors"
        @upload="goToUpload"
        @home="goHome"
      />

      <!-- Loading State -->
      <div v-if="loading" class="loading-state">
        <div class="spinner" :style="{ borderTopColor: themeColors.accent }"></div>
        <p class="loading-text" :style="{ color: themeColors.font }">
          Ładowanie galerii...
        </p>
      </div>

      <!-- Inactive Message -->
      <div v-else-if="showInactiveMessage" class="message-state">
        <div class="message-icon error">⚠️</div>
        <h2 class="message-title" :style="{ color: themeColors.font }">
          Galeria została dezaktywowana
        </h2>
        <p class="message-text" :style="{ color: themeColors.font }">
          Ta galeria jest już niedostępna.
        </p>
        <button class="btn-primary" @click="goHome">
          Strona główna
        </button>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="message-state">
        <div class="message-icon error">❌</div>
        <h2 class="message-title" :style="{ color: themeColors.font }">{{ error }}</h2>
        <button
          class="btn-primary"
          @click="handleLoadGallery"
          :style="{ backgroundColor: themeColors.accent }"
        >
          Spróbuj ponownie
        </button>
      </div>

      <!-- Photo Grid -->
      <PhotoGrid
        v-else
        :photos="photos"
        :is-empty="isEmpty"
        :theme-colors="themeColors"
        @photo-click="openLightbox"
        @upload="goToUpload"
      />

      <!-- Load More Button -->
      <div v-if="hasMore && !loading" class="load-more-container">
        <button
          class="btn-secondary"
          @click="loadMore"
          :style="{
            borderColor: themeColors.accent,
            color: themeColors.accent
          }"
        >
          Załaduj więcej
        </button>
      </div>
    </div>

    <!-- Lightbox -->
    <PhotoLightbox
      v-model="lightboxOpen"
      :photo="selectedPhoto"
      :has-previous="selectedPhotoIndex > 0"
      :has-next="selectedPhotoIndex < photos.length - 1"
      @download="handleDownloadPhoto"
      @previous="goToPreviousPhoto"
      @next="goToNextPhoto"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useClientData } from '../composables/useClientData'
import { useGallery } from '../composables/useGallery'
import AOS from 'aos'

import GalleryHeader from '../components/gallery/GalleryHeader.vue'
import GalleryStatusBanners from '../components/gallery/GalleryStatusBanners.vue'
import GalleryActions from '../components/gallery/GalleryActions.vue'
import PhotoGrid from '../components/gallery/PhotoGrid.vue'
import PhotoLightbox from '../components/gallery/PhotoLightbox.vue'

const route = useRoute()
const router = useRouter()
const guid = route.params.guid

// Composables
const {
  loading: clientLoading,
  error: clientError,
  isInactive,
  client,
  isExpired,
  isActive,
  canUpload,
  themeColors,
  loadClient,
  showExpiredWarning
} = useClientData(guid)

const {
  photos,
  loading: galleryLoading,
  error: galleryError,
  hasMore,
  isEmpty,
  loadPhotos,
  loadMore: loadMorePhotos,
  downloadPhoto
} = useGallery(guid)

// Local state
const lightboxOpen = ref(false)
const selectedPhoto = ref(null)
const selectedPhotoIndex = ref(-1)
const showInactiveMessage = ref(false)

// Computed
const loading = computed(() => clientLoading.value || galleryLoading.value)
const error = computed(() => clientError.value || galleryError.value)

const pageStyle = computed(() => ({
  background: `linear-gradient(135deg, ${themeColors.value.background} 0%, ${themeColors.value.backgroundSecondary} 100%)`,
  minHeight: '100vh',
  fontFamily: themeColors.value.fontType || 'Poppins, sans-serif'
}))

// Methods
const handleLoadGallery = async () => {
  const clientResult = await loadClient()

  if (!clientResult.success) {
    if (clientResult.status === 400) {
      showInactiveMessage.value = true
    }
    return
  }

  // Check if inactive
  if (!isActive.value) {
    showInactiveMessage.value = true
    return
  }

  // Show expired warning if needed
  showExpiredWarning()

  // Load photos
  await loadPhotos(true)
}

const loadMore = async () => {
  await loadMorePhotos()
}

const openLightbox = (photo) => {
  selectedPhoto.value = photo
  selectedPhotoIndex.value = photos.value.findIndex(p => p.id === photo.id)
  lightboxOpen.value = true
}

const goToPreviousPhoto = () => {
  if (selectedPhotoIndex.value > 0) {
    selectedPhotoIndex.value--
    selectedPhoto.value = photos.value[selectedPhotoIndex.value]
  }
}

const goToNextPhoto = () => {
  if (selectedPhotoIndex.value < photos.value.length - 1) {
    selectedPhotoIndex.value++
    selectedPhoto.value = photos.value[selectedPhotoIndex.value]
  }
}

const handleDownloadPhoto = async (photo) => {
  await downloadPhoto(photo)
}

const goToUpload = () => {
  router.push({ name: 'SendPhotos', params: { guid } })
}

const goHome = () => {
  router.push({ name: 'Home' })
}

// Lifecycle
onMounted(() => {
  AOS.init({
    duration: 600,
    easing: 'ease-out-cubic',
    once: true,
    offset: 50
  })

  handleLoadGallery()

  // Update document title with event info
  if (client.value) {
    document.title = `${client.value.eventName} - Galeria ${client.value.eventTypeDisplayName} | Snap Events`
  }
})
</script>

<style scoped>
.gallery-page {
  min-height: 100vh;
  padding: 40px 24px 80px;
}

.container {
  max-width: 1400px;
  margin: 0 auto;
}

/* Loading State */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 24px;
}

.spinner {
  width: 50px;
  height: 50px;
  border: 4px solid rgba(201, 168, 143, 0.2);
  border-top-color: #C9A88F;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.loading-text {
  margin-top: 20px;
  font-size: 16px;
  font-weight: 500;
}

/* Message State */
.message-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 24px;
  text-align: center;
}

.message-icon {
  font-size: 64px;
  margin-bottom: 24px;
}

.message-title {
  font-size: 24px;
  font-weight: 700;
  margin: 0 0 12px 0;
}

.message-text {
  font-size: 16px;
  margin: 0 0 32px 0;
  opacity: 0.8;
}

/* Buttons */
.btn-primary {
  padding: 14px 32px;
  background: linear-gradient(135deg, #C9A88F 0%, #D4AF37 100%);
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 16px rgba(201, 168, 143, 0.3);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(201, 168, 143, 0.4);
}

.btn-primary:active {
  transform: translateY(0);
}

.btn-secondary {
  padding: 14px 32px;
  background: white;
  color: #C9A88F;
  border: 2px solid #C9A88F;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
}

.btn-secondary:hover {
  background: #C9A88F;
  color: white;
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(201, 168, 143, 0.3);
}

.btn-secondary:active {
  transform: translateY(0);
}

/* Load More */
.load-more-container {
  display: flex;
  justify-content: center;
  margin-top: 48px;
}

/* Responsive */
@media (max-width: 768px) {
  .gallery-page {
    padding: 24px 16px 60px;
  }

  .message-title {
    font-size: 20px;
  }

  .message-text {
    font-size: 14px;
  }
}
</style>
