<template>
  <div
    class="upload-area"
    @click="$emit('click')"
    @dragover.prevent="isDragging = true"
    @dragleave.prevent="isDragging = false"
    @drop.prevent="handleDrop"
    :class="{ 'dragging': isDragging }"
    :style="uploadAreaStyle"
    data-aos="fade-up"
  >
    <div class="upload-icon-wrapper">
      <div class="upload-icon">
        {{ isDragging ? '📥' : '📸' }}
      </div>
      <div class="icon-background"></div>
    </div>

    <h3 class="upload-title">
      {{ isDragging ? 'Upuść pliki tutaj' : 'Dodaj zdjęcia' }}
    </h3>

    <p class="upload-description">
      {{ isDragging ? 'Puść, aby dodać zdjęcia' : 'Kliknij tutaj lub przeciągnij pliki' }}
    </p>

    <div class="upload-formats">
      <span class="format-badge">JPG</span>
      <span class="format-badge">PNG</span>
      <span class="format-badge">GIF</span>
      <span class="format-badge">WEBP</span>
    </div>

    <div class="upload-hint">
      <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
        <path d="M8 1v14M1 8h14" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
      </svg>
      <span>Maksymalnie 10 zdjęć na raz</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import type { ThemeColors } from '../../types/types'

const props = defineProps<{
  themeColors: ThemeColors
}>()

const emit = defineEmits(['click', 'files-dropped'])

const isDragging = ref(false)

const uploadAreaStyle = computed(() => {
  if (isDragging.value) {
    return {
      borderColor: props.themeColors.accent,
      background: `linear-gradient(135deg, rgba(255, 255, 255, 0.98) 0%, ${props.themeColors.accent}10 100%)`,
      transform: 'scale(1.02)'
    }
  }
  return {}
})

const handleDrop = (event: DragEvent) => {
  isDragging.value = false
  const files = event.dataTransfer?.files
  if (files && files.length > 0) {
    emit('files-dropped', files)
  }
}
</script>

<style scoped>
.upload-area {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px 40px;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.98) 0%, rgba(255, 255, 255, 0.95) 100%);
  backdrop-filter: blur(10px);
  border: 3px dashed rgba(45, 45, 45, 0.15);
  border-radius: 20px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  margin-bottom: 32px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

.upload-area::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: radial-gradient(circle at 50% 50%, rgba(45, 45, 45, 0.02) 0%, transparent 70%);
  pointer-events: none;
}

.upload-area:hover {
  border-color: rgba(45, 45, 45, 0.25);
  background: linear-gradient(135deg, rgba(255, 255, 255, 1) 0%, rgba(255, 255, 255, 0.98) 100%);
  transform: translateY(-3px);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
}

.upload-area.dragging {
  transform: scale(1.03) !important;
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.2) !important;
  border-style: solid;
}

.upload-icon-wrapper {
  position: relative;
  margin-bottom: 24px;
}

.upload-icon {
  font-size: 72px;
  position: relative;
  z-index: 2;
  transition: transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
  filter: drop-shadow(0 2px 8px rgba(0, 0, 0, 0.1));
}

.upload-area:hover .upload-icon {
  transform: scale(1.15) rotate(-5deg);
}

.upload-area.dragging .upload-icon {
  transform: scale(1.2) rotate(0deg);
  animation: bounce 0.6s ease-in-out;
}

@keyframes bounce {
  0%, 100% { transform: scale(1.2) translateY(0); }
  50% { transform: scale(1.25) translateY(-10px); }
}

.icon-background {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 100px;
  height: 100px;
  background: radial-gradient(circle, rgba(45, 45, 45, 0.03) 0%, transparent 70%);
  border-radius: 50%;
  z-index: 1;
  transition: all 0.3s ease;
}

.upload-area:hover .icon-background {
  width: 120px;
  height: 120px;
  background: radial-gradient(circle, rgba(45, 45, 45, 0.05) 0%, transparent 70%);
}

.upload-title {
  font-size: 24px;
  font-weight: 700;
  margin: 0 0 8px 0;
  text-align: center;
  color: #2D2D2D;
}

.upload-description {
  font-size: 15px;
  margin: 0 0 24px 0;
  color: #666;
  text-align: center;
  font-weight: 500;
}

.upload-formats {
  display: flex;
  gap: 8px;
  margin-bottom: 16px;
  flex-wrap: wrap;
  justify-content: center;
}

.format-badge {
  padding: 4px 12px;
  background: rgba(45, 45, 45, 0.06);
  border: 1px solid rgba(45, 45, 45, 0.1);
  border-radius: 6px;
  font-size: 11px;
  font-weight: 700;
  color: #666;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  transition: all 0.2s ease;
}

.upload-area:hover .format-badge {
  background: rgba(45, 45, 45, 0.08);
  border-color: rgba(45, 45, 45, 0.15);
}

.upload-hint {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: rgba(45, 45, 45, 0.04);
  border-radius: 8px;
  font-size: 13px;
  color: #666;
  font-weight: 500;
}

.upload-hint svg {
  flex-shrink: 0;
  opacity: 0.6;
}

@media (max-width: 768px) {
  .upload-area {
    padding: 36px 24px;
  }

  .upload-icon {
    font-size: 64px;
  }

  .upload-title {
    font-size: 22px;
  }

  .upload-description {
    font-size: 14px;
  }

  .upload-hint {
    font-size: 12px;
  }
}

@media (max-width: 480px) {
  .upload-area {
    padding: 28px 20px;
  }

  .upload-icon {
    font-size: 56px;
  }

  .upload-title {
    font-size: 20px;
  }

  .upload-description {
    font-size: 13px;
  }

  .format-badge {
    font-size: 10px;
    padding: 3px 10px;
  }
}
</style>
