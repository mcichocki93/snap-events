<template>
  <div class="app-container">
    <NotificationContainer />
    <header v-if="!isAdminRoute" class="app-header" :class="{ 'scrolled': isScrolled }">
      <div class="header-content">
        <router-link to="/" class="logo-link">
          <SnapEventsLogo :show-text="true" :show-tagline="false" />
        </router-link>

        <nav class="main-nav" :class="{ 'mobile-open': mobileMenuOpen }">
          <router-link to="/" class="nav-link" @click="closeMobileMenu">Strona główna</router-link>
          <a href="/#pricing" class="nav-link" @click="scrollToPricing">Cennik</a>
          <router-link to="/kontakt" class="nav-link" @click="closeMobileMenu">Kontakt</router-link>
        </nav>

        <button class="mobile-menu-toggle" @click="toggleMobileMenu" aria-label="Toggle menu">
          <svg v-if="!mobileMenuOpen" width="24" height="24" viewBox="0 0 24 24" fill="none">
            <path d="M3 12h18M3 6h18M3 18h18" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          </svg>
          <svg v-else width="24" height="24" viewBox="0 0 24 24" fill="none">
            <path d="M18 6L6 18M6 6l12 12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          </svg>
        </button>
      </div>
    </header>

    <main :class="isAdminRoute ? '' : 'app-main'">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import SnapEventsLogo from './components/SnapEventsLogo.vue'
import NotificationContainer from './components/NotificationContainer.vue'

const router = useRouter()
const route = useRoute()
const isScrolled = ref(false)
const isAdminRoute = computed(() => route.path.startsWith('/admin'))
const mobileMenuOpen = ref(false)

const handleScroll = () => {
  isScrolled.value = window.scrollY > 50
}

const toggleMobileMenu = () => {
  mobileMenuOpen.value = !mobileMenuOpen.value
}

const closeMobileMenu = () => {
  mobileMenuOpen.value = false
}

const scrollToPricing = (e: Event) => {
  e.preventDefault()
  closeMobileMenu()

  if (router.currentRoute.value.path !== '/') {
    router.push('/')
    setTimeout(() => {
      const pricingSection = document.getElementById('pricing')
      pricingSection?.scrollIntoView({ behavior: 'smooth' })
    }, 100)
  } else {
    const pricingSection = document.getElementById('pricing')
    pricingSection?.scrollIntoView({ behavior: 'smooth' })
  }
}

onMounted(() => {
  window.addEventListener('scroll', handleScroll)
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
})
</script>

<style>
:root {
  /* Colors */
  --color-primary: #C9A88F;
  --color-primary-dark: #B89775;
  --color-secondary: #E8D5C4;
  --color-accent: #D4AF37;
  --color-text: #2D2D2D;
  --color-text-light: #6B6B6B;
  --color-bg: #FFFFFF;
  --color-bg-light: #FAF8F6;

  /* Typography - only 2 fonts */
  --font-body: 'Poppins', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  --font-heading: 'Playfair Display', Georgia, serif;
}

* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  margin: 0;
  font-family: var(--font-body);
  color: var(--color-text);
  background: var(--color-bg);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

/* Global heading styles */
h1, h2, h3 {
  font-family: var(--font-heading);
}

.app-container {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

/* Header Styles */
.app-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1000;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  transition: all 0.3s ease;
  border-bottom: 1px solid transparent;
}

.app-header.scrolled {
  background: rgba(255, 255, 255, 0.98);
  border-bottom-color: rgba(201, 168, 143, 0.1);
  box-shadow: 0 2px 16px rgba(0, 0, 0, 0.08);
}

.header-content {
  max-width: 1400px;
  margin: 0 auto;
  padding: 16px 32px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.logo-link {
  text-decoration: none;
  display: flex;
  align-items: center;
  transition: transform 0.3s ease;
}

.logo-link:hover {
  transform: scale(1.02);
}

/* Navigation */
.main-nav {
  display: flex;
  align-items: center;
  gap: 32px;
}

.nav-link {
  text-decoration: none;
  color: var(--color-text);
  font-size: 15px;
  font-weight: 500;
  transition: all 0.3s ease;
  position: relative;
  padding: 8px 0;
}

.nav-link::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  width: 0;
  height: 2px;
  background: linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%);
  transition: width 0.3s ease;
}

.nav-link:hover {
  color: var(--color-primary);
}

.nav-link:hover::after {
  width: 100%;
}

.nav-link.router-link-active {
  color: var(--color-primary);
}

.nav-link.router-link-active::after {
  width: 100%;
}

.nav-link-secondary {
  background: var(--color-primary);
  color: white;
  padding: 10px 24px;
  border-radius: 8px;
  transition: all 0.3s ease;
}

.nav-link-secondary::after {
  display: none;
}

.nav-link-secondary:hover {
  background: var(--color-primary-dark);
  color: white;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(201, 168, 143, 0.3);
}

/* Mobile Menu Toggle */
.mobile-menu-toggle {
  display: none;
  background: none;
  border: none;
  color: var(--color-text);
  cursor: pointer;
  padding: 8px;
  border-radius: 8px;
  transition: all 0.3s ease;
}

.mobile-menu-toggle:hover {
  background: var(--color-bg-light);
}

/* Main Content */
.app-main {
  flex: 1;
  margin-top: 80px;
}

/* Responsive */
@media (max-width: 968px) {
  .header-content {
    padding: 16px 24px;
  }

  .mobile-menu-toggle {
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .main-nav {
    position: fixed;
    top: 80px;
    left: 0;
    right: 0;
    background: white;
    flex-direction: column;
    align-items: stretch;
    gap: 0;
    padding: 24px;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
    transform: translateY(-120%);
    opacity: 0;
    transition: all 0.3s ease;
    pointer-events: none;
  }

  .main-nav.mobile-open {
    transform: translateY(0);
    opacity: 1;
    pointer-events: all;
  }

  .nav-link {
    padding: 16px;
    border-bottom: 1px solid var(--color-bg-light);
    text-align: center;
  }

  .nav-link::after {
    display: none;
  }

  .nav-link-secondary {
    margin-top: 8px;
    text-align: center;
  }
}

@media (max-width: 640px) {
  .header-content {
    padding: 12px 16px;
  }

  .app-main {
    margin-top: 70px;
  }

  .main-nav {
    top: 70px;
  }
}
</style>