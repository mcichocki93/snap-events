<template>
  <div class="snap-events-logo" :class="variant">
    <svg class="logo-icon" viewBox="0 0 120 120" fill="none" xmlns="http://www.w3.org/2000/svg">
      <!-- Gradient definition -->
      <defs>
        <linearGradient id="gradient-primary" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" style="stop-color:#C9A88F;stop-opacity:1" />
          <stop offset="100%" style="stop-color:#D4AF37;stop-opacity:1" />
        </linearGradient>
      </defs>

      <!-- Camera aperture blades forming a stylized "S" -->
      <g class="aperture-blades">
        <!-- Blade 1 -->
        <path d="M60 20 L80 40 L60 50 Z" class="blade" style="animation-delay: 0s"/>
        <!-- Blade 2 -->
        <path d="M80 40 L90 60 L70 60 Z" class="blade" style="animation-delay: 0.1s"/>
        <!-- Blade 3 -->
        <path d="M90 60 L80 80 L70 70 Z" class="blade" style="animation-delay: 0.2s"/>
        <!-- Blade 4 -->
        <path d="M80 80 L60 90 L60 70 Z" class="blade" style="animation-delay: 0.3s"/>
        <!-- Blade 5 -->
        <path d="M60 90 L40 80 L50 70 Z" class="blade" style="animation-delay: 0.4s"/>
        <!-- Blade 6 -->
        <path d="M40 80 L30 60 L50 60 Z" class="blade" style="animation-delay: 0.5s"/>
        <!-- Blade 7 -->
        <path d="M30 60 L40 40 L50 50 Z" class="blade" style="animation-delay: 0.6s"/>
        <!-- Blade 8 -->
        <path d="M40 40 L60 30 L60 50 Z" class="blade" style="animation-delay: 0.7s"/>
      </g>

      <!-- Central circle with snap effect -->
      <circle cx="60" cy="60" r="15" class="center-circle"/>

      <!-- Sparkle effect -->
      <g class="sparkle" style="animation-delay: 0.8s">
        <path d="M60 10 L62 18 L70 20 L62 22 L60 30 L58 22 L50 20 L58 18 Z" class="spark"/>
      </g>
    </svg>

    <div class="logo-text" v-if="showText">
      <span class="brand-name">Snap Events</span>
      <span class="tagline" v-if="showTagline">{{ tagline }}</span>
    </div>
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
  gap: 16px;
}

/* Logo Icon */
.logo-icon {
  width: 48px;
  height: 48px;
  filter: drop-shadow(0 2px 8px rgba(201, 168, 143, 0.3));
}

.aperture-blades .blade {
  fill: url(#gradient-primary);
  transition: all 0.3s ease;
}

@keyframes bladeOpen {
  from {
    opacity: 0;
    transform: scale(0.8) rotate(-10deg);
  }
  to {
    opacity: 1;
    transform: scale(1) rotate(0deg);
  }
}

.blade {
  animation: bladeOpen 0.6s ease-out backwards;
  transform-origin: 60px 60px;
}

.center-circle {
  fill: #D4AF37;
  stroke: #C9A88F;
  stroke-width: 2;
  animation: pulse 2s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(1.1);
    opacity: 0.8;
  }
}

.sparkle {
  animation: sparkleAnim 1.5s ease-in-out infinite;
  transform-origin: 60px 20px;
}

.spark {
  fill: #D4AF37;
  filter: drop-shadow(0 0 4px rgba(212, 175, 55, 0.8));
}

@keyframes sparkleAnim {
  0%, 100% {
    opacity: 0;
    transform: scale(0.5) rotate(0deg);
  }
  50% {
    opacity: 1;
    transform: scale(1) rotate(180deg);
  }
}

.snap-events-logo:hover .blade {
  fill: #D4AF37;
  transform: scale(1.05);
}

.snap-events-logo:hover .center-circle {
  fill: #C9A88F;
  animation: none;
  transform: scale(1.2);
}

/* Logo Text */
.logo-text {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.brand-name {
  font-family: 'Poppins', sans-serif;
  font-size: 28px;
  font-weight: 700;
  background: linear-gradient(135deg, #C9A88F 0%, #D4AF37 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  letter-spacing: -0.5px;
}

.tagline {
  font-size: 12px;
  color: #6B6B6B;
  font-weight: 500;
  letter-spacing: 0.5px;
}

/* Variants */
.snap-events-logo.light .blade {
  fill: white;
}

.snap-events-logo.light .center-circle {
  fill: white;
  stroke: rgba(255, 255, 255, 0.5);
}

.snap-events-logo.light .brand-name {
  background: white;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.snap-events-logo.light .tagline {
  color: rgba(255, 255, 255, 0.8);
}

.snap-events-logo.dark .blade {
  fill: #2D2D2D;
}

.snap-events-logo.dark .center-circle {
  fill: #2D2D2D;
  stroke: #1a1a1a;
}

.snap-events-logo.dark .brand-name {
  background: #2D2D2D;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.snap-events-logo.minimal {
  gap: 12px;
}

.snap-events-logo.minimal .logo-icon {
  width: 32px;
  height: 32px;
}

.snap-events-logo.minimal .brand-name {
  font-size: 20px;
}
</style>
