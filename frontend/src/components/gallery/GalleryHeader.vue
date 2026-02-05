<template>
  <div class="gallery-header" data-aos="fade-up">
    <div class="event-badge" :style="{ background: badgeGradient }">
      <span class="event-emoji">{{ eventEmoji }}</span>
      <span class="event-type">{{ eventTypeDisplay }}</span>
    </div>

    <h1 class="event-name" :style="{ color: themeColors.font }">
      {{ eventName || 'Galeria Zdjęć' }}
    </h1>

    <p v-if="eventDate" class="event-date" :style="{ color: themeColors.font }">
      {{ formattedDate }}
    </p>

    <div class="decorative-line" :style="{ background: lineGradient }"></div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { ThemeColors } from '../../types/types'

const props = defineProps<{
  eventName?: string
  eventTypeDisplay?: string
  eventEmoji?: string
  eventDate?: string
  themeColors: ThemeColors
}>()

const formattedDate = computed(() => {
  if (!props.eventDate) return ''

  try {
    const date = new Date(props.eventDate)
    return date.toLocaleDateString('pl-PL', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    })
  } catch {
    return ''
  }
})

const badgeGradient = computed(() =>
  `linear-gradient(135deg, ${props.themeColors.accent} 0%, ${props.themeColors.accent}dd 100%)`
)

const lineGradient = computed(() =>
  `linear-gradient(90deg, transparent 0%, ${props.themeColors.accent}40 20%, ${props.themeColors.accent}80 50%, ${props.themeColors.accent}40 80%, transparent 100%)`
)
</script>

<style scoped>
.gallery-header {
  text-align: center;
  margin-bottom: 48px;
  padding: 32px 24px;
}

.event-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 20px;
  border-radius: 24px;
  margin-bottom: 20px;
  color: white;
  font-weight: 600;
  font-size: 14px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.event-emoji {
  font-size: 20px;
}

.event-type {
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.event-name {
  font-size: 48px;
  font-weight: 700;
  margin: 0 0 12px 0;
  line-height: 1.2;
  text-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
}

.event-date {
  font-size: 18px;
  margin: 0 0 24px 0;
  opacity: 0.95;
  font-weight: 500;
  text-shadow: 0 1px 4px rgba(0, 0, 0, 0.15);
}

.decorative-line {
  width: 120px;
  height: 3px;
  margin: 0 auto;
  border-radius: 2px;
}

@media (max-width: 768px) {
  .gallery-header {
    padding: 24px 16px;
    margin-bottom: 32px;
  }

  .event-name {
    font-size: 36px;
  }

  .event-date {
    font-size: 16px;
  }

  .event-badge {
    font-size: 13px;
    padding: 6px 16px;
  }

  .event-emoji {
    font-size: 18px;
  }
}

@media (max-width: 480px) {
  .event-name {
    font-size: 28px;
  }

  .event-date {
    font-size: 14px;
  }
}
</style>
