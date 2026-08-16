<template>
  <!--
    Stays on screen until dismissed. A batch usually finishes while the guest is
    looking at something else, so a timed toast told them nothing.
  -->
  <div
    class="upload-summary"
    :class="{ 'has-failures': result.failed > 0 }"
    role="status"
    data-aos="fade-up"
  >
    <div class="summary-icon" :style="{ background: accentBackground }">
      {{ result.failed > 0 ? '!' : '✓' }}
    </div>

    <div class="summary-content">
      <h3 class="summary-title" :style="{ color: themeColors.font }">
        {{ title }}
      </h3>
      <p class="summary-text" :style="{ color: themeColors.font }">
        {{ description }}
      </p>
    </div>

    <div class="summary-actions">
      <button
        class="summary-button primary"
        :style="{ background: themeColors.accent }"
        @click="$emit('gallery')"
      >
        Zobacz galerię
      </button>
      <button
        class="summary-button secondary"
        :style="{ borderColor: themeColors.accent, color: themeColors.accent }"
        @click="$emit('new-batch')"
      >
        Wyślij kolejne
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { ThemeColors } from '../../types/types'
import type { UploadBatchResult } from '../../composables/usePhotoUpload'

const props = defineProps<{
  result: UploadBatchResult
  themeColors: ThemeColors
}>()

defineEmits(['gallery', 'new-batch'])

function photoWord(count: number): string {
  if (count === 1) return 'zdjęcie'
  const lastTwo = count % 100
  const last = count % 10
  const manyForm = lastTwo >= 12 && lastTwo <= 14
  return !manyForm && last >= 2 && last <= 4 ? 'zdjęcia' : 'zdjęć'
}

const title = computed(() => {
  if (props.result.uploaded === 0) return 'Nie udało się wysłać'
  return `Wysłano ${props.result.uploaded} ${photoWord(props.result.uploaded)}`
})

const description = computed(() => {
  if (props.result.uploaded === 0) {
    return 'Żadne zdjęcie nie dotarło. Sprawdź połączenie i spróbuj ponownie.'
  }

  if (props.result.failed > 0) {
    return `${props.result.failed} ${photoWord(props.result.failed)} nie przeszło — zostały na liście, możesz wysłać je ponownie.`
  }

  return 'Wszystkie zdjęcia są już w galerii.'
})

const accentBackground = computed(() =>
  props.result.failed > 0 || props.result.uploaded === 0 ? '#c2703f' : '#4a8c5f'
)
</script>

<style scoped>
.upload-summary {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 16px;
  padding: 20px 24px;
  margin-bottom: 24px;
  background: rgba(255, 255, 255, 0.92);
  border-radius: 16px;
  border-left: 4px solid #4a8c5f;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

.upload-summary.has-failures {
  border-left-color: #c2703f;
}

.summary-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  color: #fff;
  font-size: 20px;
  font-weight: 700;
}

.summary-content {
  flex: 1 1 220px;
  min-width: 0;
}

.summary-title {
  margin: 0 0 4px 0;
  font-size: 18px;
  font-weight: 700;
}

.summary-text {
  margin: 0;
  font-size: 14px;
  opacity: 0.75;
  line-height: 1.5;
}

.summary-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.summary-button {
  padding: 10px 18px;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.summary-button.primary {
  border: none;
  color: #fff;
}

.summary-button.secondary {
  background: transparent;
  border: 2px solid;
}

.summary-button:hover {
  transform: translateY(-1px);
}

.summary-button:focus-visible {
  outline: 3px solid currentColor;
  outline-offset: 2px;
}

@media (prefers-reduced-motion: reduce) {
  .summary-button {
    transition: none;
  }
  .summary-button:hover {
    transform: none;
  }
}

@media (max-width: 560px) {
  .upload-summary {
    padding: 18px;
  }
  .summary-actions {
    width: 100%;
  }
  .summary-button {
    flex: 1 1 auto;
  }
}
</style>
