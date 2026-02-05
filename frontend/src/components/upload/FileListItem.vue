<template>
  <div class="file-item">
    <div class="file-icon">
      <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
        <rect x="3" y="3" width="18" height="18" rx="2" stroke="currentColor" stroke-width="2"/>
        <circle cx="8.5" cy="8.5" r="1.5" fill="currentColor"/>
        <path d="M21 15l-5-5L5 21" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>
    </div>

    <div class="file-info">
      <span class="file-name">{{ file.name }}</span>
      <span class="file-size">{{ formatFileSize(file.size) }}</span>

      <!-- Upload progress -->
      <div v-if="file.uploading" class="progress-container">
        <div class="progress-bar">
          <div
            class="progress-fill"
            :style="{
              width: `${file.progress}%`,
              background: `linear-gradient(90deg, ${themeColors.accent}, ${themeColors.accent}dd)`
            }"
          ></div>
        </div>
        <span class="progress-text">{{ file.progress }}%</span>
      </div>
    </div>

    <!-- Status icons -->
    <div v-if="file.uploaded" class="status-icon success">
      <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
        <path d="M16.667 5L7.5 14.167 3.333 10" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>
    </div>

    <div v-else-if="file.error" class="status-icon error">
      <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
        <circle cx="10" cy="10" r="8" stroke="currentColor" stroke-width="2"/>
        <path d="M10 6v5M10 13.5v.5" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
      </svg>
    </div>

    <!-- Remove button -->
    <button
      v-else-if="!uploading && !file.uploaded"
      class="remove-btn"
      @click="$emit('remove', index)"
      aria-label="Usuń plik"
    >
      <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
        <path d="M13.5 4.5L4.5 13.5M4.5 4.5l9 9" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
      </svg>
    </button>
  </div>
</template>

<script setup lang="ts">
import type { FileUpload, ThemeColors } from '../../types/types'

defineProps<{
  file: FileUpload
  index: number
  uploading: boolean
  themeColors: ThemeColors
}>()

defineEmits(['remove'])

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
}
</script>

<style scoped>
.file-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  background: #f8f9fa;
  border-radius: 10px;
  transition: all 0.2s ease;
}

.file-item:hover {
  background: #f0f1f3;
}

.file-icon {
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(201, 168, 143, 0.1);
  border-radius: 8px;
  color: #C9A88F;
}

.file-info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.file-name {
  font-size: 14px;
  font-weight: 500;
  color: #2D2D2D;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.file-size {
  font-size: 12px;
  color: #6B6B6B;
}

.progress-container {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 4px;
}

.progress-bar {
  flex: 1;
  height: 4px;
  background: #e0e0e0;
  border-radius: 2px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  border-radius: 2px;
  transition: width 0.3s ease;
}

.progress-text {
  font-size: 11px;
  color: #6B6B6B;
  min-width: 32px;
  text-align: right;
}

.status-icon {
  flex-shrink: 0;
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
}

.status-icon.success {
  background: rgba(16, 185, 129, 0.1);
  color: #10B981;
}

.status-icon.error {
  background: rgba(239, 68, 68, 0.1);
  color: #EF4444;
}

.remove-btn {
  flex-shrink: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: none;
  border: none;
  border-radius: 50%;
  color: #999;
  cursor: pointer;
  transition: all 0.2s ease;
}

.remove-btn:hover {
  background: rgba(239, 68, 68, 0.1);
  color: #EF4444;
}

@media (max-width: 480px) {
  .file-item {
    padding: 10px 12px;
    gap: 10px;
  }

  .file-icon {
    width: 36px;
    height: 36px;
  }

  .file-name {
    font-size: 13px;
  }
}
</style>
