<template>
  <div class="status-banners">
    <!-- Expired Warning Banner -->
    <div
      v-if="isExpired && !isInactive"
      class="status-banner warning"
      data-aos="fade-down"
    >
      <div class="banner-icon">⚠️</div>
      <div class="banner-content">
        <h3 class="banner-title">Link do galerii wygasł</h3>
        <p class="banner-text">
          Możesz przeglądać i pobierać zdjęcia, ale nie możesz już dodawać nowych.
        </p>
      </div>
    </div>

    <!-- Inactive Warning Banner -->
    <div
      v-if="isInactive"
      class="status-banner error"
      data-aos="fade-down"
    >
      <div class="banner-icon">🚫</div>
      <div class="banner-content">
        <h3 class="banner-title">Galeria została dezaktywowana</h3>
        <p class="banner-text">
          Ta galeria jest już niedostępna.
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps({
  isExpired: {
    type: Boolean,
    default: false
  },
  isInactive: {
    type: Boolean,
    default: false
  }
})
</script>

<style scoped>
.status-banners {
  margin-bottom: 24px;
}

.status-banner {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  padding: 20px 24px;
  border-radius: 16px;
  margin-bottom: 16px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
  animation: slideDown 0.4s ease-out;
}

.status-banner.warning {
  background: linear-gradient(135deg, #f59e0b 0%, #fbbf24 100%);
  color: white;
}

.status-banner.error {
  background: linear-gradient(135deg, #ef4444 0%, #f87171 100%);
  color: white;
}

.banner-icon {
  font-size: 32px;
  flex-shrink: 0;
}

.banner-content {
  flex: 1;
}

.banner-title {
  font-size: 18px;
  font-weight: 700;
  margin: 0 0 8px 0;
}

.banner-text {
  font-size: 14px;
  margin: 0;
  opacity: 0.95;
  line-height: 1.5;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 768px) {
  .status-banner {
    padding: 16px 20px;
  }

  .banner-icon {
    font-size: 28px;
  }

  .banner-title {
    font-size: 16px;
  }

  .banner-text {
    font-size: 13px;
  }
}
</style>
