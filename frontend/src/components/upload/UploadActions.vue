<template>
  <div class="navigation-buttons" data-aos="fade-up">
    <button
      class="nav-button primary"
      @click="$emit('gallery')"
      :style="buttonStyle"
    >
      <svg class="button-icon" width="20" height="20" viewBox="0 0 20 20" fill="none">
        <rect x="3" y="3" width="6" height="6" rx="1" stroke="currentColor" stroke-width="2"/>
        <rect x="11" y="3" width="6" height="6" rx="1" stroke="currentColor" stroke-width="2"/>
        <rect x="3" y="11" width="6" height="6" rx="1" stroke="currentColor" stroke-width="2"/>
        <rect x="11" y="11" width="6" height="6" rx="1" stroke="currentColor" stroke-width="2"/>
      </svg>
      <span>Zobacz galerię</span>
    </button>

    <button
      class="nav-button secondary"
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
  themeColors: ThemeColors
}>()

defineEmits(['gallery', 'home'])

const buttonStyle = computed(() => ({
  background: `linear-gradient(135deg, ${props.themeColors.accent} 0%, ${props.themeColors.accent}dd 100%)`,
  color: 'white'
}))
</script>

<style scoped>
.navigation-buttons {
  display: flex;
  justify-content: center;
  gap: 16px;
  margin-top: 48px;
  flex-wrap: wrap;
}

.nav-button {
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
  min-width: 180px;
  justify-content: center;
}

.nav-button.primary {
  color: white;
}

.nav-button.primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(201, 168, 143, 0.3);
}

.nav-button.secondary {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  color: #2D2D2D;
  border: 2px solid rgba(255, 255, 255, 0.5);
}

.nav-button.secondary:hover {
  background: rgba(255, 255, 255, 1);
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
  border-color: rgba(255, 255, 255, 0.8);
}

.nav-button:active {
  transform: translateY(0);
}

.button-icon {
  flex-shrink: 0;
}

@media (max-width: 768px) {
  .navigation-buttons {
    flex-direction: column;
    align-items: stretch;
  }

  .nav-button {
    width: 100%;
    min-width: unset;
  }
}
</style>
