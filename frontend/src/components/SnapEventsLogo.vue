<template>
  <div class="snap-events-logo" :class="variant">
    <img
      src="/logo.png"
      alt="Snap Events Logo"
      class="logo-image"
      :class="{ 'small': !showText }"
    />
  </div>
</template>

<script setup lang="ts">
interface Props {
  variant?: 'default' | 'light' | 'dark' | 'minimal'
  showText?: boolean
  showTagline?: boolean
  tagline?: string
}

withDefaults(defineProps<Props>(), {
  variant: 'default',
  showText: true,
  showTagline: false,
  tagline: 'Każde zdjęcie to wspomnienie'
})
</script>

<style scoped>
.snap-events-logo {
  display: inline-flex;
  align-items: center;
}

.logo-image {
  height: 80px;
  width: auto;
  object-fit: contain;
  transition: transform 0.3s ease;
}

.logo-image.small {
  height: 60px;
}

.snap-events-logo:hover .logo-image {
  transform: scale(1.02);
}

/* Variants */
.snap-events-logo.minimal .logo-image {
  height: 40px;
}

.snap-events-logo.light .logo-image {
  filter: brightness(0) invert(1);
}

.snap-events-logo.dark .logo-image {
  filter: brightness(0);
}

@media (max-width: 640px) {
  .logo-image {
    height: 60px;
  }

  .logo-image.small {
    height: 48px;
  }
}
</style>
