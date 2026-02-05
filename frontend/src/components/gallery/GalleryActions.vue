<template>
  <div class="action-buttons" data-aos="fade-up">
    <button
      class="action-button primary"
      @click="$emit('upload')"
      :disabled="!canUpload || isExpired || !isActive"
      :style="buttonStyle"
    >
      <svg class="button-icon" width="20" height="20" viewBox="0 0 20 20" fill="none">
        <path d="M10 4v12M4 10h12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
      </svg>
      <span>{{ uploadButtonLabel }}</span>
    </button>

    <button
      class="action-button secondary"
      @click="$emit('home')"
    >
      <svg class="button-icon" width="20" height="20" viewBox="0 0 20 20" fill="none">
        <path d="M3 10l7-7 7 7M5 8v9h10V8" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>
      <span>Strona główna</span>
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { ThemeColors } from '../../types/types'

const props = defineProps<{
  canUpload: boolean
  isExpired: boolean
  isActive: boolean
  themeColors: ThemeColors
}>()

defineEmits(['upload', 'home'])

const uploadButtonLabel = computed(() => {
  if (props.isExpired) {
    return 'Upload niedostępny (wygasło)'
  }
  return 'Prześlij zdjęcia'
})

const buttonStyle = computed(() => {
  if (!props.canUpload || props.isExpired || !props.isActive) {
    return {
      background: '#e5e5e5',
      color: '#999',
      cursor: 'not-allowed'
    }
  }
  return {
    background: `linear-gradient(135deg, ${props.themeColors.accent} 0%, ${props.themeColors.accent}dd 100%)`,
    color: 'white'
  }
})
</script>

<style scoped>
.action-buttons {
  display: flex;
  justify-content: center;
  gap: 16px;
  margin-bottom: 48px;
  flex-wrap: wrap;
}

.action-button {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 14px 28px;
  border: none;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
  min-width: 200px;
  justify-content: center;
}

.action-button.primary {
  color: white;
}

.action-button.primary:not(:disabled):hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(201, 168, 143, 0.3);
}

.action-button.primary:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.action-button.secondary {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  color: #2D2D2D;
  border: 2px solid rgba(255, 255, 255, 0.5);
}

.action-button.secondary:hover {
  background: rgba(255, 255, 255, 1);
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
  border-color: rgba(255, 255, 255, 0.8);
}

.action-button:active {
  transform: translateY(0);
}

.button-icon {
  flex-shrink: 0;
}

@media (max-width: 768px) {
  .action-buttons {
    flex-direction: column;
    align-items: stretch;
    padding: 0 20px;
  }

  .action-button {
    width: 100%;
    min-width: unset;
  }
}
</style>
