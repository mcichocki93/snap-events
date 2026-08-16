<template>
  <!--
    Shown when a previous visit left photos unsent - typically because the phone
    discarded the tab while it was in the background. The photos themselves are
    still in the browser's storage, so finishing costs the guest one tap rather
    than hunting through their camera roll again.
  -->
  <div class="interrupted" role="status" data-aos="fade-up">
    <div class="interrupted-icon">↻</div>

    <div class="interrupted-content">
      <h3 class="interrupted-title" :style="{ color: themeColors.font }">
        Masz niedokończoną wysyłkę
      </h3>
      <p class="interrupted-text" :style="{ color: themeColors.font }">
        {{ description }}
      </p>
    </div>

    <div class="interrupted-actions">
      <button
        class="interrupted-button primary"
        :style="{ background: themeColors.accent }"
        @click="$emit('resume')"
      >
        Dokończ wysyłkę
      </button>
      <button
        class="interrupted-button secondary"
        :style="{ borderColor: themeColors.accent, color: themeColors.accent }"
        @click="$emit('discard')"
      >
        Odrzuć
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { ThemeColors } from '../../types/types'
import type { StoredQueue } from '../../services/uploadQueueStore'

const props = defineProps<{
  queue: StoredQueue
  themeColors: ThemeColors
}>()

defineEmits(['resume', 'discard'])

function photoWord(count: number): string {
  if (count === 1) return 'zdjęcie'
  const lastTwo = count % 100
  const last = count % 10
  const manyForm = lastTwo >= 12 && lastTwo <= 14
  return !manyForm && last >= 2 && last <= 4 ? 'zdjęcia' : 'zdjęć'
}

const description = computed(() => {
  const pending = props.queue.items.filter(item => item.status !== 'done').length
  const done = props.queue.items.length - pending

  if (done === 0) {
    return `${pending} ${photoWord(pending)} czeka na wysłanie.`
  }

  return `Wysłano ${done} z ${props.queue.items.length}. Zostało ${pending} ${photoWord(pending)}.`
})
</script>

<style scoped>
.interrupted {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 16px;
  padding: 20px 24px;
  margin-bottom: 24px;
  background: rgba(255, 255, 255, 0.92);
  border-radius: 16px;
  border-left: 4px solid #b98b4e;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

.interrupted-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: #b98b4e;
  color: #fff;
  font-size: 22px;
  font-weight: 700;
}

.interrupted-content {
  flex: 1 1 220px;
  min-width: 0;
}

.interrupted-title {
  margin: 0 0 4px 0;
  font-size: 18px;
  font-weight: 700;
}

.interrupted-text {
  margin: 0;
  font-size: 14px;
  opacity: 0.75;
  line-height: 1.5;
}

.interrupted-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.interrupted-button {
  padding: 10px 18px;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s ease;
}

.interrupted-button.primary {
  border: none;
  color: #fff;
}

.interrupted-button.secondary {
  background: transparent;
  border: 2px solid;
}

.interrupted-button:hover {
  transform: translateY(-1px);
}

.interrupted-button:focus-visible {
  outline: 3px solid currentColor;
  outline-offset: 2px;
}

@media (prefers-reduced-motion: reduce) {
  .interrupted-button {
    transition: none;
  }
  .interrupted-button:hover {
    transform: none;
  }
}

@media (max-width: 560px) {
  .interrupted {
    padding: 18px;
  }
  .interrupted-actions {
    width: 100%;
  }
  .interrupted-button {
    flex: 1 1 auto;
  }
}
</style>
