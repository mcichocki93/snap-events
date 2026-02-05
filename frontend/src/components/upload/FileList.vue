<template>
  <div v-if="files.length > 0" class="file-list-section" data-aos="fade-up">
    <div class="file-list-header">
      <h3 class="file-list-title">
        Wybrane pliki: {{ files.length }}
      </h3>
      <span
        class="file-count-badge"
        :class="{ 'error': files.length > maxFiles }"
      >
        {{ files.length > maxFiles ? `Przekroczono limit ${maxFiles} zdjęć!` : `${files.length} / ${maxFiles}` }}
      </span>
    </div>

    <div class="file-list">
      <FileListItem
        v-for="(file, index) in files"
        :key="index"
        :file="file"
        :index="index"
        :theme-colors="themeColors"
        :uploading="uploading"
        @remove="$emit('remove', index)"
      />
    </div>

    <!-- Upload Button -->
    <button
      class="upload-btn"
      :class="{ 'disabled': !canUpload }"
      :disabled="!canUpload || uploading"
      @click="$emit('upload')"
      :style="{ background: canUpload ? `linear-gradient(135deg, ${themeColors.accent} 0%, ${themeColors.accent}dd 100%)` : undefined }"
    >
      <span v-if="uploading" class="btn-loading">
        <span class="spinner"></span>
        Przesyłanie...
      </span>
      <span v-else class="btn-content">
        <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
          <path d="M10 2v12M4 8l6-6 6 6" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
          <path d="M2 14v3a1 1 0 001 1h14a1 1 0 001-1v-3" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
        </svg>
        Prześlij zdjęcia
      </span>
    </button>
  </div>
</template>

<script setup lang="ts">
import FileListItem from './FileListItem.vue'
import type { FileUpload, ThemeColors } from '../../types/types'

defineProps<{
  files: FileUpload[]
  maxFiles: number
  canUpload: boolean
  uploading: boolean
  themeColors: ThemeColors
}>()

defineEmits(['remove', 'upload'])
</script>

<style scoped>
.file-list-section {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-radius: 16px;
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 2px 16px rgba(0, 0, 0, 0.08);
}

.file-list-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
  flex-wrap: wrap;
  gap: 12px;
}

.file-list-title {
  font-size: 18px;
  font-weight: 600;
  color: #2D2D2D;
  margin: 0;
}

.file-count-badge {
  padding: 6px 14px;
  background: linear-gradient(135deg, #10B981 0%, #059669 100%);
  color: white;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 600;
}

.file-count-badge.error {
  background: linear-gradient(135deg, #EF4444 0%, #DC2626 100%);
}

.file-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 24px;
}

.upload-btn {
  width: 100%;
  padding: 16px 32px;
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

.upload-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 24px rgba(201, 168, 143, 0.4);
}

.upload-btn:active:not(:disabled) {
  transform: translateY(0);
}

.upload-btn.disabled,
.upload-btn:disabled {
  background: #E0E0E0;
  color: #999;
  cursor: not-allowed;
  box-shadow: none;
}

.btn-content,
.btn-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
}

.spinner {
  width: 20px;
  height: 20px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 480px) {
  .file-list-section {
    padding: 16px;
  }

  .file-list-title {
    font-size: 16px;
  }

  .upload-btn {
    padding: 14px 24px;
    font-size: 15px;
  }
}
</style>
