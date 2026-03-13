<template>
  <Teleport to="body">
    <div v-if="modelValue" class="lightbox-overlay" @click.self="close">
      <div class="lightbox-container">
        <!-- Header -->
        <div class="lightbox-header">
          <span class="lightbox-title">{{ photo?.name }}</span>
          <button class="lightbox-close" @click="close" aria-label="Zamknij">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
              <path d="M18 6L6 18M6 6l12 12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
            </svg>
          </button>
        </div>

        <!-- Image -->
        <div class="lightbox-content">
          <img
            v-if="photo"
            :src="photo.fullUrl"
            :alt="photo.name"
            class="lightbox-image"
            @load="onImageLoad"
          />
          <div v-if="loading" class="lightbox-loading">
            <div class="spinner"></div>
          </div>
        </div>

        <!-- Actions -->
        <div class="lightbox-actions">
          <button class="download-btn" :style="{ background: `linear-gradient(135deg, ${accentColor} 0%, ${accentColor}cc 100%)` }" @click="handleDownload">
            <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
              <path d="M10 2v12M4 10l6 6 6-6" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
              <path d="M2 16v1a1 1 0 001 1h14a1 1 0 001-1v-1" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
            </svg>
            Pobierz
          </button>
        </div>
      </div>

      <!-- Navigation arrows -->
      <button
        v-if="hasPrevious"
        class="lightbox-nav lightbox-nav-prev"
        @click="$emit('previous')"
        aria-label="Poprzednie zdjęcie"
      >
        <svg width="32" height="32" viewBox="0 0 32 32" fill="none">
          <path d="M20 24l-8-8 8-8" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>

      <button
        v-if="hasNext"
        class="lightbox-nav lightbox-nav-next"
        @click="$emit('next')"
        aria-label="Następne zdjęcie"
      >
        <svg width="32" height="32" viewBox="0 0 32 32" fill="none">
          <path d="M12 24l8-8-8-8" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import type { PhotoInfo } from '../../types/types'

interface Props {
  modelValue: boolean
  photo: PhotoInfo | null
  hasPrevious?: boolean
  hasNext?: boolean
  accentColor?: string
}

const props = withDefaults(defineProps<Props>(), {
  hasPrevious: false,
  hasNext: false,
  accentColor: '#3b82f6'
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'download', photo: PhotoInfo): void
  (e: 'previous'): void
  (e: 'next'): void
}>()

const loading = ref(true)

const close = () => {
  emit('update:modelValue', false)
}

const handleDownload = () => {
  if (props.photo) {
    emit('download', props.photo)
  }
}

const onImageLoad = () => {
  loading.value = false
}

// Reset loading state when photo changes
watch(() => props.photo, () => {
  loading.value = true
})

// Handle keyboard navigation
const handleKeydown = (e: KeyboardEvent) => {
  if (!props.modelValue) return

  if (e.key === 'Escape') {
    close()
  } else if (e.key === 'ArrowLeft' && props.hasPrevious) {
    emit('previous')
  } else if (e.key === 'ArrowRight' && props.hasNext) {
    emit('next')
  }
}

// Prevent body scroll when lightbox is open
watch(() => props.modelValue, (isOpen) => {
  if (isOpen) {
    document.body.style.overflow = 'hidden'
  } else {
    document.body.style.overflow = ''
  }
})

onMounted(() => {
  window.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown)
  document.body.style.overflow = ''
})
</script>

<style scoped>
.lightbox-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.95);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  animation: fadeIn 0.2s ease;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.lightbox-container {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.lightbox-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 24px;
  background: rgba(0, 0, 0, 0.5);
}

.lightbox-title {
  color: white;
  font-size: 16px;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: calc(100% - 50px);
}

.lightbox-close {
  background: none;
  border: none;
  color: white;
  cursor: pointer;
  padding: 8px;
  border-radius: 50%;
  transition: background 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.lightbox-close:hover {
  background: rgba(255, 255, 255, 0.1);
}

.lightbox-content {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  position: relative;
  min-height: 0;
}

.lightbox-image {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
  animation: scaleIn 0.3s ease;
}

@keyframes scaleIn {
  from { transform: scale(0.95); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}

.lightbox-loading {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
}

.spinner {
  width: 48px;
  height: 48px;
  border: 3px solid rgba(255, 255, 255, 0.2);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.lightbox-actions {
  display: flex;
  justify-content: center;
  padding: 16px 24px;
  background: rgba(0, 0, 0, 0.5);
}

.download-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.download-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.3);
}

/* Navigation arrows */
.lightbox-nav {
  position: fixed;
  top: 50%;
  transform: translateY(-50%);
  background: rgba(255, 255, 255, 0.1);
  border: none;
  color: white;
  cursor: pointer;
  padding: 16px;
  border-radius: 50%;
  transition: all 0.2s ease;
  z-index: 10000;
}

.lightbox-nav:hover {
  background: rgba(255, 255, 255, 0.2);
}

.lightbox-nav-prev {
  left: 24px;
}

.lightbox-nav-next {
  right: 24px;
}

@media (max-width: 768px) {
  .lightbox-header {
    padding: 12px 16px;
  }

  .lightbox-title {
    font-size: 14px;
  }

  .lightbox-content {
    padding: 16px;
  }

  .lightbox-actions {
    padding: 12px 16px;
  }

  .download-btn {
    padding: 10px 20px;
    font-size: 14px;
  }

  .lightbox-nav {
    padding: 12px;
  }

  .lightbox-nav-prev {
    left: 12px;
  }

  .lightbox-nav-next {
    right: 12px;
  }
}
</style>
