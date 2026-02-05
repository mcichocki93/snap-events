<template>
  <div>
    <!-- Empty State -->
    <div v-if="isEmpty" class="empty-state" data-aos="fade-up">
      <div class="empty-icon">📷</div>
      <h2 class="empty-title" :style="{ color: themeColors.font }">
        Galeria jest pusta
      </h2>
      <p class="empty-text" :style="{ color: themeColors.font }">
        Nie ma jeszcze żadnych zdjęć w galerii
      </p>
      <button
        class="empty-button"
        @click="$emit('upload')"
        :style="{ background: themeColors.accent }"
      >
        <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
          <path d="M10 4v12M4 10h12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
        </svg>
        Dodaj pierwsze zdjęcia
      </button>
    </div>

    <!-- Photo Grid -->
    <div v-else class="photo-grid">
      <PhotoCard
        v-for="photo in photos"
        :key="photo.id"
        :photo="photo"
        @click="$emit('photo-click', photo)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import PhotoCard from './PhotoCard.vue'
import type { PhotoInfo, ThemeColors } from '../../types/types'

defineProps<{
  photos: PhotoInfo[]
  isEmpty: boolean
  themeColors: ThemeColors
}>()

defineEmits(['photo-click', 'upload'])
</script>

<style scoped>
/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 24px;
  text-align: center;
}

.empty-icon {
  font-size: 80px;
  margin-bottom: 24px;
  opacity: 0.5;
}

.empty-title {
  font-size: 24px;
  font-weight: 700;
  margin: 0 0 12px 0;
}

.empty-text {
  font-size: 16px;
  margin: 0 0 32px 0;
  opacity: 0.8;
}

.empty-button {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 14px 28px;
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 16px rgba(201, 168, 143, 0.3);
}

.empty-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(201, 168, 143, 0.4);
}

.empty-button:active {
  transform: translateY(0);
}

/* Photo Grid */
.photo-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 24px;
  margin-top: 20px;
}

@media (max-width: 768px) {
  .photo-grid {
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
    gap: 16px;
  }

  .empty-icon {
    font-size: 64px;
  }

  .empty-title {
    font-size: 20px;
  }

  .empty-text {
    font-size: 14px;
  }
}

@media (max-width: 480px) {
  .photo-grid {
    grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
    gap: 12px;
  }
}
</style>
