<template>
  <div class="photo-card" @click="handleClick">
    <div class="photo-wrapper">
      <img
        :src="photo.thumbnailUrl"
        :alt="photo.name"
        class="photo-image"
        loading="lazy"
        @load="onLoad"
        @error="onError"
      />

      <!-- Loading state -->
      <div v-if="loading" class="photo-loading">
        <div class="spinner"></div>
      </div>

      <!-- Error state -->
      <div v-if="hasError" class="photo-error">
        <svg width="48" height="48" viewBox="0 0 48 48" fill="none">
          <rect x="6" y="6" width="36" height="36" rx="4" stroke="currentColor" stroke-width="2"/>
          <path d="M6 36l12-12 8 8 8-12 8 12" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
          <line x1="18" y1="18" x2="30" y2="30" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          <line x1="30" y1="18" x2="18" y2="30" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
        </svg>
      </div>

      <!-- Hover Overlay -->
      <div class="photo-overlay">
        <svg width="48" height="48" viewBox="0 0 48 48" fill="none">
          <circle cx="20" cy="20" r="12" stroke="currentColor" stroke-width="2"/>
          <path d="M28 28l10 10" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          <path d="M20 14v12M14 20h12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
        </svg>
      </div>
    </div>

    <!-- Photo Info -->
    <div class="photo-info">
      <div class="photo-name">{{ photo.name }}</div>
      <div class="photo-size">{{ formatFileSize(photo.size) }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { PhotoInfo } from '../../types/types'

interface Props {
  photo: PhotoInfo
}

const props = defineProps<Props>()

interface Emits {
  (e: 'click', photo: PhotoInfo): void
}

const emit = defineEmits<Emits>()

const loading = ref(true)
const hasError = ref(false)

const handleClick = (): void => {
  emit('click', props.photo)
}

const onLoad = (): void => {
  loading.value = false
}

const onError = (): void => {
  loading.value = false
  hasError.value = true
}

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
}
</script>

<style scoped>
.photo-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
}

.photo-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
}

.photo-wrapper {
  position: relative;
  padding-top: 100%; /* 1:1 aspect ratio */
  overflow: hidden;
  background: #f5f5f5;
}

.photo-image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.photo-card:hover .photo-image {
  transform: scale(1.05);
}

.photo-loading,
.photo-error {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f5f5;
}

.photo-error {
  color: #999;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid rgba(201, 168, 143, 0.2);
  border-top-color: #C9A88F;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.photo-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.3s ease;
  pointer-events: none;
  color: white;
}

.photo-card:hover .photo-overlay {
  opacity: 1;
}

.photo-info {
  padding: 12px 14px;
  background: white;
}

.photo-name {
  font-weight: 500;
  font-size: 14px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-bottom: 4px;
  color: #2D2D2D;
}

.photo-size {
  font-size: 12px;
  color: #6B6B6B;
}

@media (max-width: 640px) {
  .photo-card {
    border-radius: 8px;
  }

  .photo-info {
    padding: 10px 12px;
  }

  .photo-name {
    font-size: 13px;
  }
}
</style>
