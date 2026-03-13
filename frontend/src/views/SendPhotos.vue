<template>
  <div class="send-photos-page" :style="pageStyle">
    <div class="container">
      <!-- Header -->
      <UploadHeader
        :event-name="client?.eventName"
        :event-type-display="client?.eventTypeDisplayName"
        :event-emoji="client?.eventTypeEmoji"
        :event-date="client?.eventDate"
        :theme-colors="themeColors"
      />

      <!-- Info Card -->
      <UploadInfoCard
        v-if="client"
        :max-files="client.maxFiles"
        :max-file-size="client.maxFileSize"
        :selected-count="selectedFiles.length"
        :theme-colors="themeColors"
      />

      <!-- Upload Area -->
      <UploadArea
        :theme-colors="themeColors"
        @click="selectFiles"
        @files-dropped="handleFilesDropped"
      />

      <!-- Hidden File Input -->
      <input
        ref="fileInput"
        type="file"
        multiple
        accept="image/*"
        @change="handleFileSelect"
        style="display: none"
      />

      <!-- Selected Files List -->
      <FileList
        :files="selectedFiles"
        :max-files="client?.maxFiles || 10"
        :can-upload="canUpload"
        :uploading="uploading"
        :theme-colors="themeColors"
        @remove="removeFile"
        @upload="uploadFiles"
      />

      <!-- Navigation -->
      <UploadActions
        :theme-colors="themeColors"
        @gallery="goToGallery"
        @home="goHome"
      />
    </div>

    <!-- Success Toast -->
    <Transition name="toast">
      <div v-if="showSuccessToast" class="toast success">
        <div class="toast-icon">✓</div>
        <div class="toast-content">
          <h4 class="toast-title">Sukces!</h4>
          <p class="toast-message">{{ successMessage }}</p>
        </div>
      </div>
    </Transition>

    <!-- Error Toast -->
    <Transition name="toast">
      <div v-if="showErrorToast" class="toast error">
        <div class="toast-icon">✕</div>
        <div class="toast-content">
          <h4 class="toast-title">Błąd</h4>
          <p class="toast-message">{{ errorMessage }}</p>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useClientData } from '../composables/useClientData'
import { usePhotoUpload } from '../composables/usePhotoUpload'
import AOS from 'aos'

// Components
import UploadHeader from '../components/upload/UploadHeader.vue'
import UploadInfoCard from '../components/upload/UploadInfoCard.vue'
import UploadArea from '../components/upload/UploadArea.vue'
import FileList from '../components/upload/FileList.vue'
import UploadActions from '../components/upload/UploadActions.vue'

const route = useRoute()
const router = useRouter()
const guid = Array.isArray(route.params.guid) ? route.params.guid[0] : route.params.guid

// Composables
const {
  client,
  isExpired,
  isActive,
  themeColors,
  loadClient
} = useClientData(guid)

const {
  selectedFiles,
  uploading,
  canUpload,
  addFiles,
  removeFile,
  uploadFiles: uploadFilesComposable
} = usePhotoUpload(guid, client)

// Local state
const fileInput = ref(null)
const showSuccessToast = ref(false)
const showErrorToast = ref(false)
const successMessage = ref('')
const errorMessage = ref('')

// Computed
const pageStyle = computed(() => ({
  background: `linear-gradient(135deg, ${themeColors.value.background} 0%, ${themeColors.value.backgroundSecondary} 100%)`,
  minHeight: '100vh',
  fontFamily: themeColors.value.fontType || 'Poppins, sans-serif'
}))

// Methods
const selectFiles = () => {
  fileInput.value?.click()
}

const handleFileSelect = (event) => {
  const files = event.target.files
  if (files && files.length > 0) {
    addFiles(files)
  }
  // Reset input
  event.target.value = ''
}

const handleFilesDropped = (files) => {
  addFiles(files)
}

const uploadFiles = async () => {
  const result = await uploadFilesComposable()

  if (result.success) {
    successMessage.value = `Pomyślnie przesłano ${result.data?.count ?? 0} zdjęć!`
    showSuccessToast.value = true
    setTimeout(() => {
      showSuccessToast.value = false
    }, 5000)
  } else {
    errorMessage.value = result.message || 'Nie udało się przesłać zdjęć'
    showErrorToast.value = true
    setTimeout(() => {
      showErrorToast.value = false
    }, 5000)
  }
}

const goToGallery = () => {
  router.push({ name: 'Gallery', params: { guid } })
}

const goHome = () => {
  router.push({ name: 'Home' })
}

// Lifecycle
onMounted(async () => {
  AOS.init({
    duration: 600,
    easing: 'ease-out-cubic',
    once: true,
    offset: 50
  })

  const result = await loadClient()

  if (!result.success) {
    router.push({ name: 'Home' })
    return
  }

  // Check if expired or inactive
  if (!isActive.value) {
    errorMessage.value = 'Galeria została dezaktywowana'
    showErrorToast.value = true
    setTimeout(() => {
      router.push({ name: 'Gallery', params: { guid } })
    }, 3000)
    return
  }

  if (isExpired.value) {
    errorMessage.value = 'Link do galerii wygasł - nie można już dodawać zdjęć'
    showErrorToast.value = true
    setTimeout(() => {
      router.push({ name: 'Gallery', params: { guid } })
    }, 3000)
    return
  }

  // Update document title
  if (client.value) {
    document.title = `Prześlij zdjęcia - ${client.value.eventName} | Snap Events`
  }
})
</script>

<style scoped>
.send-photos-page {
  min-height: 100vh;
  padding: 40px 24px 80px;
}

.container {
  max-width: 900px;
  margin: 0 auto;
}

/* Toast Notifications */
.toast {
  position: fixed;
  top: 24px;
  right: 24px;
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 16px 20px;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
  z-index: 9999;
  max-width: 400px;
  color: white;
}

.toast.success {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
}

.toast.error {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
}

.toast-icon {
  font-size: 24px;
  font-weight: 700;
  flex-shrink: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 50%;
}

.toast-content {
  flex: 1;
}

.toast-title {
  font-size: 16px;
  font-weight: 700;
  margin: 0 0 4px 0;
}

.toast-message {
  font-size: 14px;
  margin: 0;
  opacity: 0.95;
}

/* Toast Transitions */
.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(100px);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(100px);
}

/* Responsive */
@media (max-width: 768px) {
  .send-photos-page {
    padding: 24px 16px 60px;
  }

  .toast {
    top: 16px;
    right: 16px;
    left: 16px;
    max-width: unset;
  }
}
</style>
