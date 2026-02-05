<template>
  <div class="landing-page">
    <!-- Hero Section -->
    <section class="hero">
      <div class="hero-content">
        <div class="hero-text" data-aos="fade-up">
          <!-- Logo -->
          <SnapEventsLogoAnimated :show-text="false" class="hero-logo" />
          
          <h1 class="hero-title">
            Zbierz wszystkie zdjęcia<br>
            <span class="highlight">z Twojej uroczystości</span><br>
            w jednym miejscu
          </h1>
          <p class="hero-subtitle">
            Prosta galeria na wesele, urodziny, chrzciny i każdą inną okazję.<br>
            Goście dodają zdjęcia bez instalowania aplikacji.
          </p>
          
          <!-- Event types badges -->
          <div class="event-types" data-aos="fade-up" data-aos-delay="200">
            <span class="event-badge">💍 Wesela</span>
            <span class="event-badge">🎂 Urodziny</span>
            <span class="event-badge">👶 Chrzciny</span>
            <span class="event-badge">🎉 Imprezy firmowe</span>
          </div>
          
          <button class="cta-button" @click="scrollToPricing">
            <span>Sprawdź cennik</span>
            <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
              <path d="M10 4L10 16M10 16L16 10M10 16L4 10" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </button>
        </div>
        
        <div class="hero-image" data-aos="fade-left" data-aos-delay="200">
          <div class="phone-mockup">
            <div class="phone-frame">
              <div class="phone-screen">
                <div class="gallery-preview">
                  <div class="preview-header">
                    <SnapEventsLogo variant="minimal" :show-tagline="false" />
                  </div>
                  <div class="preview-grid">
                    <div class="preview-photo" v-for="(photo, i) in samplePhotos" :key="i">
                      <img :src="photo" :alt="`Sample photo ${i + 1}`" />
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Decorative elements -->
      <div class="hero-decoration">
        <div class="circle circle-1"></div>
        <div class="circle circle-2"></div>
        <div class="circle circle-3"></div>
      </div>
    </section>

    <!-- How It Works Section -->
    <section class="how-it-works">
      <div class="container">
        <h2 class="section-title" data-aos="fade-up">Jak to działa?</h2>
        <p class="section-subtitle" data-aos="fade-up" data-aos-delay="100">
          Cztery proste kroki do zebrania wszystkich wspomnień
        </p>
        
        <div class="steps">
          <div class="step" v-for="(step, index) in steps" :key="index"
               data-aos="fade-up" :data-aos-delay="index * 100">
            <div class="step-number">{{ index + 1 }}</div>
            <div class="step-icon" v-html="step.icon"></div>
            <h3 class="step-title">{{ step.title }}</h3>
            <p class="step-description">{{ step.description }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Pricing Section -->
    <section class="pricing" id="pricing">
      <div class="container">
        <h2 class="section-title" data-aos="fade-up">Wybierz pakiet dla siebie</h2>
        <p class="section-subtitle" data-aos="fade-up" data-aos-delay="100">
          Płatność jednorazowa • Bez ukrytych kosztów • Dla każdej uroczystości
        </p>
        
        <div class="pricing-cards">
          <div class="pricing-card" v-for="(pkg, index) in packages" :key="index"
               :class="{ featured: pkg.featured }"
               data-aos="fade-up" :data-aos-delay="index * 100">
            <div class="card-badge" v-if="pkg.badge">{{ pkg.badge }}</div>
            <div class="card-header">
              <h3 class="card-title">{{ pkg.name }}</h3>
              <div class="card-price">
                <span class="price-amount">{{ pkg.price }}</span>
                <span class="price-currency">PLN</span>
              </div>
              <p class="card-subtitle">{{ pkg.subtitle }}</p>
            </div>
            
            <div class="card-features">
              <div class="feature" v-for="(feature, i) in pkg.features" :key="i">
                <svg class="feature-icon" width="20" height="20" viewBox="0 0 20 20" fill="none">
                  <path d="M16.6667 5L7.50004 14.1667L3.33337 10" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                </svg>
                <span>{{ feature }}</span>
              </div>
            </div>
            
            <button class="card-button" :class="{ primary: pkg.featured }" @click="contactUs">
              Wybierz {{ pkg.name }}
            </button>
          </div>
        </div>
      </div>
    </section>

    <!-- Use Cases Section -->
    <section class="use-cases">
      <div class="container">
        <h2 class="section-title" data-aos="fade-up">Idealne na każdą okazję</h2>
        
        <div class="use-cases-grid">
          <div class="use-case" v-for="(useCase, index) in useCases" :key="index"
               data-aos="fade-up" :data-aos-delay="index * 50">
            <div class="use-case-icon">{{ useCase.emoji }}</div>
            <h3 class="use-case-title">{{ useCase.title }}</h3>
            <p class="use-case-description">{{ useCase.description }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- CTA Section -->
    <section class="final-cta">
      <div class="container">
        <div class="cta-content" data-aos="zoom-in">
          <SnapEventsLogo variant="light" :show-tagline="false" class="cta-logo" />
          <h2 class="cta-title">Gotowy na swoją uroczystość?</h2>
          <p class="cta-text">
            Skontaktuj się z nami i zabezpiecz swoją datę
          </p>
          <button class="cta-button large" @click="contactUs">
            Skontaktuj się
          </button>
        </div>
      </div>
    </section>

    <!-- Footer -->
    <footer class="footer">
      <div class="container">
        <div class="footer-content">
          <div class="footer-brand">
            <SnapEventsLogo :show-tagline="true" />
            <p class="footer-description">
              Profesjonalna galeria zdjęć dla Twoich najważniejszych chwil.
              Wesela, urodziny, chrzciny i każda inna uroczystość.
            </p>
          </div>
          <div class="footer-sections">
            <div class="footer-section">
              <h4>Informacje</h4>
              <ul>
                <li><router-link to="/regulamin">Regulamin</router-link></li>
                <li><router-link to="/prywatnosc">Polityka prywatności</router-link></li>
                <li><router-link to="/kontakt">Kontakt</router-link></li>
                <li><router-link to="/faq">FAQ</router-link></li>
              </ul>
            </div>
          </div>
        </div>
        <div class="footer-bottom">
          <p>&copy; 2026 Snap Events. Wszystkie prawa zastrzeżone.</p>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import AOS from 'aos'
import 'aos/dist/aos.css'
import SnapEventsLogo from '../components/SnapEventsLogo.vue'
import SnapEventsLogoAnimated from '../components/SnapEventsLogoAnimated.vue'

const router = useRouter()

// Sample photos for phone mockup preview
const samplePhotos = [
  'https://images.unsplash.com/photo-1519741497674-611481863552?w=400&h=400&fit=crop',
  'https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=400&h=400&fit=crop',
  'https://images.unsplash.com/photo-1530103862676-de8c9debad1d?w=400&h=400&fit=crop',
  'https://images.unsplash.com/photo-1511285560929-80b456fea0bc?w=400&h=400&fit=crop',
  'https://images.unsplash.com/photo-1478146896981-b80fe463b330?w=400&h=400&fit=crop',
  'https://images.unsplash.com/photo-1505236858219-8359eb29e329?w=400&h=400&fit=crop'
]

// Icons as SVG strings
const PackageIcon = `<svg width="48" height="48" viewBox="0 0 48 48" fill="none">
  <rect x="8" y="8" width="32" height="32" rx="4" stroke="currentColor" stroke-width="2"/>
  <path d="M8 20h32M20 8v32" stroke="currentColor" stroke-width="2"/>
</svg>`

const EmailIcon = `<svg width="48" height="48" viewBox="0 0 48 48" fill="none">
  <rect x="6" y="12" width="36" height="24" rx="4" stroke="currentColor" stroke-width="2"/>
  <path d="M6 16l18 12 18-12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
</svg>`

const ShareIcon = `<svg width="48" height="48" viewBox="0 0 48 48" fill="none">
  <circle cx="12" cy="24" r="4" stroke="currentColor" stroke-width="2"/>
  <circle cx="36" cy="12" r="4" stroke="currentColor" stroke-width="2"/>
  <circle cx="36" cy="36" r="4" stroke="currentColor" stroke-width="2"/>
  <path d="M15.5 22l17-8M15.5 26l17 8" stroke="currentColor" stroke-width="2"/>
</svg>`

const CloudIcon = `<svg width="48" height="48" viewBox="0 0 48 48" fill="none">
  <path d="M36 30c4.418 0 8-3.582 8-8 0-4.084-3.056-7.45-7-7.938C35.636 8.778 30.328 4 24 4c-7.18 0-13 5.82-13 13 0 .341.013.68.039 1.016C6.778 18.636 4 21.944 4 26c0 4.418 3.582 8 8 8h24z" stroke="currentColor" stroke-width="2"/>
  <path d="M24 28v12M20 36l4 4 4-4" stroke="currentColor" stroke-width="2"/>
</svg>`

const steps = [
  {
    icon: PackageIcon,
    title: 'Wybierz pakiet',
    description: 'Dopasuj do swoich potrzeb i otrzymaj unikalny link'
  },
  {
    icon: EmailIcon,
    title: 'Otrzymaj link',
    description: 'Email z linkiem i QR code gotowy do wydruku'
  },
  {
    icon: ShareIcon,
    title: 'Udostępnij gościom',
    description: 'QR na stole, link w zaproszeniu lub SMS'
  },
  {
    icon: CloudIcon,
    title: 'Zbierz wspomnienia',
    description: 'Wszystko automatycznie w Google Drive'
  }
]

const packages = [
  {
    name: 'Starter',
    price: '99',
    subtitle: 'Idealne na małe uroczystości',
    badge: null,
    featured: false,
    features: [
      '150 zdjęć',
      '14 dni dostępności',
      'Domyślny motyw',
      'Email z linkiem + QR code',
      'Google Drive storage'
    ]
  },
  {
    name: 'Standard',
    price: '199',
    subtitle: 'Najpopularniejszy wybór',
    badge: 'Polecany',
    featured: true,
    features: [
      'Bez limitu zdjęć',
      '30 dni dostępności',
      'Domyślny motyw',
      'Email z linkiem + QR code',
      'Google Drive storage'
    ]
  },
  {
    name: 'Premium',
    price: '349',
    subtitle: 'Pełna personalizacja',
    badge: null,
    featured: false,
    features: [
      'Bez limitu zdjęć',
      '90 dni dostępności',
      'Kolory, czcionki, tekst na miarę',
      'Email z linkiem + QR code',
      'Google Drive storage'
    ]
  }
]

const useCases = [
  {
    emoji: '💍',
    title: 'Wesela',
    description: 'Zbierz wszystkie zdjęcia od gości w jednym miejscu. Łatwe udostępnianie przez QR code.'
  },
  {
    emoji: '🎂',
    title: 'Urodziny',
    description: 'Każdy gość może dodać swoje zdjęcia z imprezy. Wspomnienia na lata!'
  },
  {
    emoji: '👶',
    title: 'Chrzciny',
    description: 'Rodzina i przyjaciele dzielą się zdjęciami z tej wyjątkowej chwili.'
  },
  {
    emoji: '🎓',
    title: 'Komunie',
    description: 'Galeria dla rodziny i znajomych. Proste udostępnianie linku.'
  },
  {
    emoji: '🎉',
    title: 'Imprezy firmowe',
    description: 'Eventy, konferencje, integracje - wszystkie zdjęcia w jednym miejscu.'
  },
  {
    emoji: '🎊',
    title: 'Inne uroczystości',
    description: 'Baby shower, wieczory panieńskie, reuniony - na każdą okazję!'
  }
]

const scrollToPricing = () => {
  const pricingSection = document.getElementById('pricing')
  pricingSection?.scrollIntoView({ behavior: 'smooth' })
}

const contactUs = () => {
  router.push('/kontakt')
}

onMounted(() => {
  AOS.init({
    duration: 800,
    easing: 'ease-out-cubic',
    once: true,
    offset: 50
  })
  
  document.title = 'Snap Events - Galeria zdjęć na wesele, urodziny i każdą uroczystość'
  
  const metaDescription = document.querySelector('meta[name="description"]')
  if (metaDescription) {
    metaDescription.setAttribute('content', 'Prosta galeria zdjęć dla Twojej uroczystości. Goście dodają zdjęcia bez aplikacji. Idealne na wesela, urodziny, chrzciny. Od 99 PLN.')
  }
})
</script>

<style scoped>
:root {
  --color-primary: #C9A88F;
  --color-primary-dark: #B89775;
  --color-secondary: #E8D5C4;
  --color-accent: #D4AF37;
  --color-text: #2D2D2D;
  --color-text-light: #6B6B6B;
  --color-bg: #FFFFFF;
  --color-bg-light: #FAF8F6;
  --shadow-sm: 0 2px 8px rgba(0, 0, 0, 0.08);
  --shadow-md: 0 4px 16px rgba(0, 0, 0, 0.12);
  --shadow-lg: 0 8px 32px rgba(0, 0, 0, 0.16);
}

.landing-page {
  color: var(--color-text);
  overflow-x: hidden;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
}

/* Hero Section */
.hero {
  position: relative;
  min-height: 100vh;
  display: flex;
  align-items: center;
  background: linear-gradient(135deg, var(--color-bg-light) 0%, #FFF 50%, var(--color-secondary) 100%);
  overflow: hidden;
  padding: 80px 24px;
}

.hero-content {
  max-width: 1200px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 80px;
  align-items: center;
  position: relative;
  z-index: 2;
}

.hero-text {
  animation: fadeInUp 1s ease-out;
}

.hero-logo {
  margin-bottom: 32px;
  animation: fadeInDown 1s ease-out;
}

@keyframes fadeInDown {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.hero-title {
  font-size: 56px;
  font-weight: 700;
  line-height: 1.2;
  margin: 0 0 24px 0;
  color: var(--color-text);
}

.hero-title .highlight {
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-accent) 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.hero-subtitle {
  font-size: 20px;
  line-height: 1.6;
  color: var(--color-text-light);
  margin: 0 0 32px 0;
}

.event-types {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 32px;
}

.event-badge {
  display: inline-flex;
  align-items: center;
  padding: 8px 16px;
  background: rgba(201, 168, 143, 0.1);
  border: 1px solid rgba(201, 168, 143, 0.3);
  border-radius: 20px;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text);
  transition: all 0.3s ease;
}

.event-badge:hover {
  background: rgba(201, 168, 143, 0.2);
  border-color: var(--color-primary);
  transform: translateY(-2px);
}

.cta-button {
  display: inline-flex;
  align-items: center;
  gap: 12px;
  padding: 16px 32px;
  background: var(--color-primary);
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 18px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: var(--shadow-md);
}

.cta-button:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-lg);
  background: var(--color-primary-dark);
}

.cta-button.large {
  padding: 20px 48px;
  font-size: 20px;
}

.cta-button svg {
  transition: transform 0.3s ease;
}

.cta-button:hover svg {
  transform: translateY(2px);
}

/* Phone Mockup */
.hero-image {
  position: relative;
  animation: fadeInRight 1s ease-out 0.3s both;
}

.phone-mockup {
  position: relative;
  perspective: 1000px;
}

.phone-frame {
  width: 300px;
  height: 600px;
  background: #1F1F1F;
  border-radius: 36px;
  padding: 12px;
  box-shadow: 0 24px 48px rgba(0, 0, 0, 0.3);
  position: relative;
  animation: float 6s ease-in-out infinite;
}

@keyframes float {
  0%, 100% { transform: translateY(0px) rotate(0deg); }
  50% { transform: translateY(-20px) rotate(-2deg); }
}

.phone-screen {
  width: 100%;
  height: 100%;
  background: white;
  border-radius: 28px;
  overflow: hidden;
}

.gallery-preview {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.preview-header {
  padding: 16px;
  border-bottom: 1px solid #f0f0f0;
  flex-shrink: 0;
}

.preview-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  padding: 20px;
}

.preview-photo {
  aspect-ratio: 1;
  background: linear-gradient(135deg, var(--color-secondary) 0%, var(--color-primary) 100%);
  border-radius: 12px;
  overflow: hidden;
  position: relative;
}

.preview-photo img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  animation: fadeInPhoto 0.6s ease-out;
  animation-delay: calc(var(--i) * 0.1s);
  animation-fill-mode: both;
}

@keyframes fadeInPhoto {
  from {
    opacity: 0;
    transform: scale(0.9);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.preview-photo:nth-child(1) { --i: 1; }
.preview-photo:nth-child(2) { --i: 2; }
.preview-photo:nth-child(3) { --i: 3; }
.preview-photo:nth-child(4) { --i: 4; }
.preview-photo:nth-child(5) { --i: 5; }
.preview-photo:nth-child(6) { --i: 6; }

/* Decorative Elements */
.hero-decoration {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
  z-index: 1;
}

.circle {
  position: absolute;
  border-radius: 50%;
  background: radial-gradient(circle, var(--color-accent) 0%, transparent 70%);
  opacity: 0.1;
  animation: pulse 8s ease-in-out infinite;
}

.circle-1 {
  width: 400px;
  height: 400px;
  top: -100px;
  right: -100px;
  animation-delay: 0s;
}

.circle-2 {
  width: 300px;
  height: 300px;
  bottom: -50px;
  left: -50px;
  animation-delay: 2s;
}

.circle-3 {
  width: 200px;
  height: 200px;
  top: 50%;
  left: 50%;
  animation-delay: 4s;
}

@keyframes pulse {
  0%, 100% {
    transform: scale(1);
    opacity: 0.1;
  }
  50% {
    transform: scale(1.2);
    opacity: 0.2;
  }
}

/* How It Works Section */
.how-it-works {
  padding: 120px 24px;
  background: var(--color-bg);
}

.section-title {
  font-size: 48px;
  font-weight: 700;
  text-align: center;
  margin: 0 0 16px 0;
  color: var(--color-text);
}

.section-subtitle {
  font-size: 20px;
  text-align: center;
  color: var(--color-text-light);
  margin: 0 0 64px 0;
}

.steps {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 48px;
  margin-top: 64px;
}

.step {
  text-align: center;
  position: relative;
}

.step-number {
  position: absolute;
  top: -16px;
  left: 50%;
  transform: translateX(-50%);
  width: 48px;
  height: 48px;
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-accent) 100%);
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  font-weight: 700;
  box-shadow: var(--shadow-md);
}

.step-icon {
  width: 80px;
  height: 80px;
  margin: 32px auto 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-bg-light);
  border-radius: 20px;
  color: var(--color-primary);
  transition: all 0.3s ease;
}

.step:hover .step-icon {
  transform: scale(1.1);
  background: var(--color-primary);
  color: white;
}

.step-title {
  font-size: 24px;
  font-weight: 600;
  margin: 0 0 12px 0;
  color: var(--color-text);
}

.step-description {
  font-size: 16px;
  line-height: 1.6;
  color: var(--color-text-light);
  margin: 0;
}

/* Pricing Section */
.pricing {
  padding: 120px 24px;
  background: var(--color-bg-light);
}

.pricing-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 32px;
  margin-top: 64px;
  max-width: 1100px;
  margin-left: auto;
  margin-right: auto;
}

.pricing-card {
  background: white;
  border-radius: 24px;
  padding: 40px;
  box-shadow: var(--shadow-sm);
  transition: all 0.3s ease;
  position: relative;
  border: 2px solid transparent;
}

.pricing-card:hover {
  transform: translateY(-8px);
  box-shadow: var(--shadow-lg);
}

.pricing-card.featured {
  border-color: var(--color-primary);
  box-shadow: var(--shadow-md);
}

.pricing-card.featured::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%);
  border-radius: 24px 24px 0 0;
}

.card-badge {
  position: absolute;
  top: -12px;
  left: 50%;
  transform: translateX(-50%);
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-accent) 100%);
  color: white;
  padding: 6px 20px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 600;
  box-shadow: var(--shadow-md);
}

.card-header {
  text-align: center;
  margin-bottom: 32px;
}

.card-title {
  font-size: 28px;
  font-weight: 700;
  margin: 0 0 16px 0;
  color: var(--color-text);
}

.card-price {
  display: flex;
  align-items: baseline;
  justify-content: center;
  gap: 8px;
}

.price-amount {
  font-size: 56px;
  font-weight: 700;
  color: var(--color-primary);
  font-family: var(--font-heading);
}

.price-currency {
  font-size: 24px;
  font-weight: 600;
  color: var(--color-text-light);
}

.card-subtitle {
  font-size: 14px;
  color: var(--color-text-light);
  margin: 8px 0 0 0;
}

.card-features {
  margin-bottom: 32px;
}

.feature {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 0;
  font-size: 16px;
  color: var(--color-text);
}

.feature-icon {
  flex-shrink: 0;
  color: var(--color-primary);
}

.card-button {
  width: 100%;
  padding: 16px;
  background: white;
  border: 2px solid var(--color-primary);
  color: var(--color-primary);
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.card-button:hover {
  background: var(--color-bg-light);
  transform: translateY(-2px);
}

.card-button.primary {
  background: var(--color-primary);
  color: white;
  border-color: var(--color-primary);
}

.card-button.primary:hover {
  background: var(--color-primary-dark);
  border-color: var(--color-primary-dark);
}

/* Use Cases Section */
.use-cases {
  padding: 120px 24px;
  background: white;
}

.use-cases-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 32px;
  margin-top: 64px;
}

.use-case {
  text-align: center;
  padding: 32px;
  background: var(--color-bg-light);
  border-radius: 16px;
  transition: all 0.3s ease;
}

.use-case:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-md);
}

.use-case-icon {
  font-size: 64px;
  margin-bottom: 16px;
  animation: float 3s ease-in-out infinite;
}

.use-case-title {
  font-size: 20px;
  font-weight: 600;
  margin: 0 0 12px 0;
  color: var(--color-text);
}

.use-case-description {
  font-size: 14px;
  line-height: 1.6;
  color: var(--color-text-light);
  margin: 0;
}

/* Final CTA Section */
.final-cta {
  padding: 120px 24px;
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-accent) 100%);
  color: white;
}

.cta-content {
  text-align: center;
  max-width: 800px;
  margin: 0 auto;
}

.cta-logo {
  margin-bottom: 24px;
}

.cta-title {
  font-size: 48px;
  font-weight: 700;
  margin: 0 0 24px 0;
}

.cta-text {
  font-size: 20px;
  margin: 0 0 40px 0;
  opacity: 0.95;
}

.final-cta .cta-button {
  background: white;
  color: var(--color-primary);
}

.final-cta .cta-button:hover {
  background: var(--color-bg-light);
}

/* Footer */
.footer {
  background: var(--color-text);
  color: white;
  padding: 60px 24px 24px;
}

.footer-content {
  display: flex;
  justify-content: space-between;
  align-items: start;
  max-width: 1200px;
  margin: 0 auto 40px;
  gap: 40px;
}

.footer-description {
  margin-top: 16px;
  font-size: 14px;
  line-height: 1.6;
  color: rgba(255, 255, 255, 0.7);
  max-width: 300px;
}

.footer-sections {
  display: flex;
  gap: 64px;
}

.footer-section h4 {
  font-size: 16px;
  font-weight: 600;
  margin: 0 0 16px 0;
  color: white;
}

.footer-section ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.footer-section ul li {
  margin-bottom: 12px;
}

.footer-section a {
  color: rgba(255, 255, 255, 0.7);
  text-decoration: none;
  font-size: 14px;
  transition: color 0.3s ease;
}

.footer-section a:hover {
  color: white;
}

.footer-bottom {
  max-width: 1200px;
  margin: 0 auto;
  padding-top: 24px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  text-align: center;
  opacity: 0.5;
  font-size: 14px;
}

/* Animations */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes fadeInRight {
  from {
    opacity: 0;
    transform: translateX(30px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

/* Responsive */
@media (max-width: 968px) {
  .hero-content {
    grid-template-columns: 1fr;
    gap: 60px;
    text-align: center;
  }

  .hero-title {
    font-size: 42px;
  }

  .hero-image {
    justify-self: center;
  }

  .event-types {
    justify-content: center;
  }

  .pricing-cards {
    grid-template-columns: 1fr;
    max-width: 400px;
  }

  .footer-content {
    flex-direction: column;
    text-align: center;
  }

  .footer-sections {
    flex-direction: column;
    gap: 32px;
  }
}

@media (max-width: 640px) {
  .hero {
    padding: 60px 24px;
  }

  .hero-title {
    font-size: 32px;
  }

  .hero-subtitle {
    font-size: 18px;
  }

  .section-title {
    font-size: 36px;
  }

  .steps {
    grid-template-columns: 1fr;
  }

  .phone-frame {
    width: 250px;
    height: 500px;
  }
}
</style>