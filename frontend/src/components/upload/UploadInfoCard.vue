<template>
  <div class="info-card" data-aos="fade-up">
    <div class="info-icon" :style="{ color: themeColors.accent }">
      ℹ️
    </div>
    <div class="info-content">
      <h3 class="info-title" :style="{ color: themeColors.font }">
        Informacje o przesyłaniu
      </h3>
      <div class="info-item" :style="{ color: themeColors.font }">
        Limit zdjęć na jeden upload: <strong>{{ maxFiles }}</strong>
      </div>
      <div v-if="selectedCount > 0" class="info-item" :style="{ color: themeColors.font }">
        Wybrano: <strong>{{ selectedCount }} / {{ maxFiles }}</strong>
      </div>
      <div class="info-item" :style="{ color: themeColors.font }">
        Maksymalny rozmiar pliku: <strong>{{ maxFileSizeMB }} MB</strong>
      </div>
      <div class="info-note" :style="{ color: themeColors.font }">
        Możesz przesyłać zdjęcia wielokrotnie bez limitu całkowitej liczby
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { ThemeColors } from '../../types/types'

const props = defineProps<{
  maxFiles: number
  maxFileSize: number
  selectedCount: number
  themeColors: ThemeColors
}>()

const maxFileSizeMB = computed(() => {
  return Math.round(props.maxFileSize / (1024 * 1024))
})
</script>

<style scoped>
.info-card {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  padding: 24px;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
  margin-bottom: 32px;
  border: 1px solid rgba(255, 255, 255, 0.3);
}

.info-icon {
  font-size: 32px;
  flex-shrink: 0;
}

.info-content {
  flex: 1;
}

.info-title {
  font-size: 18px;
  font-weight: 700;
  margin: 0 0 16px 0;
  color: #2D2D2D !important;
}

.info-item {
  font-size: 15px;
  margin-bottom: 8px;
  color: #2D2D2D !important;
  opacity: 0.9;
}

.info-item strong {
  font-weight: 600;
  color: #2D2D2D !important;
}

.info-note {
  font-size: 13px;
  margin-top: 12px;
  color: #2D2D2D !important;
  opacity: 0.7;
  font-style: italic;
}

@media (max-width: 768px) {
  .info-card {
    padding: 20px;
    gap: 12px;
  }

  .info-icon {
    font-size: 28px;
  }

  .info-title {
    font-size: 16px;
  }

  .info-item {
    font-size: 14px;
  }

  .info-note {
    font-size: 12px;
  }
}
</style>
