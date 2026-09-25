<template>
  <Transition name="consent">
    <div v-if="needsAnswer" class="consent-bar" role="dialog" aria-modal="false" aria-labelledby="consent-title">
      <div class="consent-inner">
        <div class="consent-text">
          <h2 id="consent-title" class="consent-title">Cookies analityczne</h2>
          <p>
            Chcemy wiedzieć, ile osób odwiedza stronę — używamy do tego Google Analytics.
            Bez Twojej zgody nie uruchamiamy go wcale. Do samego działania galerii
            i przesyłania zdjęć zgoda nie jest potrzebna.
            <router-link to="/prywatnosc">Polityka Prywatności</router-link>
          </p>
        </div>

        <div class="consent-actions">
          <button type="button" class="btn btn-decline" @click="decline">Nie zgadzam się</button>
          <button type="button" class="btn btn-accept" @click="accept">Zgadzam się</button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
/**
 * Asks before anything analytical runs. Both answers are one click and look like
 * equally real options, because a refusal buried in small print is not a refusal.
 * Ignoring the bar counts as neither: nothing loads until someone chooses.
 */
import { useCookieConsent } from '../composables/useCookieConsent'

const { needsAnswer, accept, decline } = useCookieConsent()
</script>

<style scoped>
.consent-bar {
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 9998;
  background: rgba(255, 255, 255, 0.98);
  backdrop-filter: blur(10px);
  border-top: 1px solid rgba(201, 168, 143, 0.5);
  box-shadow: 0 -4px 24px rgba(0, 0, 0, 0.1);
  padding: 16px 20px;
  padding-bottom: calc(16px + env(safe-area-inset-bottom, 0px));
}

.consent-inner {
  max-width: 1100px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  gap: 20px;
}

.consent-text {
  flex: 1;
}

.consent-title {
  margin: 0 0 4px 0;
  font-size: 15px;
  font-weight: 700;
  color: #2D2D2D;
}

.consent-text p {
  margin: 0;
  font-size: 13.5px;
  line-height: 1.5;
  color: #55504A;
  max-width: 72ch;
}

.consent-text a {
  color: #A8862C;
  white-space: nowrap;
}

.consent-actions {
  display: flex;
  gap: 10px;
  flex-shrink: 0;
}

.btn {
  padding: 10px 18px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  border: 1px solid transparent;
  transition: transform 0.15s ease, background 0.15s ease;
  font-family: inherit;
}

.btn:hover {
  transform: translateY(-1px);
}

.btn:focus-visible {
  outline: 2px solid #A8862C;
  outline-offset: 2px;
}

.btn-decline {
  background: transparent;
  border-color: #C9A88F;
  color: #55504A;
}

.btn-decline:hover {
  background: rgba(201, 168, 143, 0.14);
}

.btn-accept {
  background: linear-gradient(135deg, #C9A88F 0%, #B89376 100%);
  color: #fff;
}

.consent-enter-active,
.consent-leave-active {
  transition: transform 0.25s ease, opacity 0.25s ease;
}

.consent-enter-from,
.consent-leave-to {
  transform: translateY(100%);
  opacity: 0;
}

@media (prefers-reduced-motion: reduce) {
  .consent-enter-active,
  .consent-leave-active {
    transition: none;
  }
  .btn:hover {
    transform: none;
  }
}

@media (max-width: 720px) {
  .consent-inner {
    flex-direction: column;
    align-items: stretch;
    gap: 14px;
  }

  .consent-actions {
    flex-direction: row;
  }

  .consent-actions .btn {
    flex: 1;
  }
}
</style>
