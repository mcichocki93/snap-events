<template>
  <div class="contact-page">
    <div class="contact-container">
      <!-- Header -->
      <div class="contact-header" data-aos="fade-up">
        <h1 class="contact-title">Kontakt</h1>
        <p class="contact-subtitle">
          Masz pytania? Chętnie pomożemy! Wypełnij formularz, a odpowiemy najszybciej jak to możliwe.
        </p>
        <div class="decorative-line"></div>
      </div>

      <div class="contact-content">
        <!-- Contact Form -->
        <div class="contact-form-wrapper" data-aos="fade-up" data-aos-delay="100">
          <form @submit.prevent="handleSubmit" class="contact-form">
            <!-- Name -->
            <div class="form-group">
              <label for="name" class="form-label">Imię i nazwisko *</label>
              <input
                id="name"
                v-model="form.name"
                type="text"
                class="form-input"
                :class="{ 'error': errors.name }"
                placeholder="Jan Kowalski"
                required
              />
              <span v-if="errors.name" class="form-error">{{ errors.name }}</span>
            </div>

            <!-- Email -->
            <div class="form-group">
              <label for="email" class="form-label">Email *</label>
              <input
                id="email"
                v-model="form.email"
                type="email"
                class="form-input"
                :class="{ 'error': errors.email }"
                placeholder="jan@example.com"
                required
              />
              <span v-if="errors.email" class="form-error">{{ errors.email }}</span>
            </div>

            <!-- Phone -->
            <div class="form-group">
              <label for="phone" class="form-label">Telefon (opcjonalnie)</label>
              <input
                id="phone"
                v-model="form.phone"
                type="tel"
                class="form-input"
                placeholder="+48 123 456 789"
              />
            </div>

            <!-- Message -->
            <div class="form-group">
              <label for="message" class="form-label">Wiadomość *</label>
              <textarea
                id="message"
                v-model="form.message"
                class="form-textarea"
                :class="{ 'error': errors.message }"
                placeholder="Twoja wiadomość..."
                rows="5"
                required
              ></textarea>
              <span v-if="errors.message" class="form-error">{{ errors.message }}</span>
            </div>

            <!-- Privacy consent -->
            <div class="form-group checkbox-group">
              <label class="checkbox-label">
                <input
                  type="checkbox"
                  v-model="form.consent"
                  class="form-checkbox"
                  required
                />
                <span class="checkbox-custom"></span>
                <span class="checkbox-text">
                  Akceptuję <router-link to="/prywatnosc">politykę prywatności</router-link>
                  i zgadzam się na przetwarzanie moich danych osobowych. *
                </span>
              </label>
              <span v-if="errors.consent" class="form-error">{{ errors.consent }}</span>
            </div>

            <!-- Submit button -->
            <button
              type="submit"
              class="submit-button"
              :disabled="isSubmitting"
            >
              <span v-if="isSubmitting" class="button-loading">
                <span class="spinner"></span>
                Wysyłanie...
              </span>
              <span v-else class="button-content">
                <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                  <path d="M18 2L9 11M18 2l-6 16-3-7-7-3 16-6z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                </svg>
                Wyślij wiadomość
              </span>
            </button>
          </form>
        </div>

        <!-- Contact Info -->
        <div class="contact-info" data-aos="fade-up" data-aos-delay="200">
          <div class="info-card">
            <div class="info-icon">📧</div>
            <h3 class="info-title">Email</h3>
            <p class="info-text">kontakt@snapevents.pl</p>
          </div>

          <div class="info-card">
            <div class="info-icon">📱</div>
            <h3 class="info-title">Telefon</h3>
            <p class="info-text">+48 123 456 789</p>
          </div>

          <div class="info-card">
            <div class="info-icon">⏰</div>
            <h3 class="info-title">Godziny pracy</h3>
            <p class="info-text">Pon-Pt: 9:00 - 17:00</p>
          </div>

          <div class="info-card">
            <div class="info-icon">💬</div>
            <h3 class="info-title">Czas odpowiedzi</h3>
            <p class="info-text">Do 24 godzin</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Success Modal -->
    <Transition name="modal">
      <div v-if="showSuccessModal" class="modal-overlay" @click="closeSuccessModal">
        <div class="modal-content" @click.stop>
          <div class="modal-icon success">✓</div>
          <h2 class="modal-title">Wiadomość wysłana!</h2>
          <p class="modal-text">
            Dziękujemy za kontakt. Odpowiemy na Twoją wiadomość najszybciej jak to możliwe.
          </p>
          <button class="modal-button" @click="closeSuccessModal">
            Zamknij
          </button>
        </div>
      </div>
    </Transition>

    <!-- Error Modal -->
    <Transition name="modal">
      <div v-if="showErrorModal" class="modal-overlay" @click="closeErrorModal">
        <div class="modal-content" @click.stop>
          <div class="modal-icon error">✕</div>
          <h2 class="modal-title">Wystąpił błąd</h2>
          <p class="modal-text">
            {{ errorMessage }}
          </p>
          <button class="modal-button" @click="closeErrorModal">
            Spróbuj ponownie
          </button>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import AOS from 'aos'
import api from '../services/api'

// Form state
const form = reactive({
  name: '',
  email: '',
  phone: '',
  message: '',
  consent: false
})

const errors = reactive({
  name: '',
  email: '',
  message: '',
  consent: ''
})

const isSubmitting = ref(false)
const showSuccessModal = ref(false)
const showErrorModal = ref(false)
const errorMessage = ref('')

// Validation
const validateForm = (): boolean => {
  let isValid = true

  // Reset errors
  errors.name = ''
  errors.email = ''
  errors.message = ''
  errors.consent = ''

  // Name validation
  if (!form.name.trim()) {
    errors.name = 'Imię i nazwisko jest wymagane'
    isValid = false
  } else if (form.name.trim().length < 2) {
    errors.name = 'Imię musi mieć co najmniej 2 znaki'
    isValid = false
  }

  // Email validation
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!form.email.trim()) {
    errors.email = 'Email jest wymagany'
    isValid = false
  } else if (!emailRegex.test(form.email)) {
    errors.email = 'Podaj poprawny adres email'
    isValid = false
  }

  // Message validation
  if (!form.message.trim()) {
    errors.message = 'Wiadomość jest wymagana'
    isValid = false
  } else if (form.message.trim().length < 10) {
    errors.message = 'Wiadomość musi mieć co najmniej 10 znaków'
    isValid = false
  }

  // Consent validation
  if (!form.consent) {
    errors.consent = 'Musisz zaakceptować politykę prywatności'
    isValid = false
  }

  return isValid
}

// Submit handler
const handleSubmit = async () => {
  if (!validateForm()) {
    return
  }

  isSubmitting.value = true

  try {
    await api.sendContactMessage({
      name: form.name.trim(),
      email: form.email.trim(),
      phone: form.phone.trim() || undefined,
      message: form.message.trim()
    })

    // Reset form
    form.name = ''
    form.email = ''
    form.phone = ''
    form.message = ''
    form.consent = false

    showSuccessModal.value = true
  } catch (error: any) {
    errorMessage.value = error.response?.data?.message || 'Nie udało się wysłać wiadomości. Spróbuj ponownie później.'
    showErrorModal.value = true
  } finally {
    isSubmitting.value = false
  }
}

const closeSuccessModal = () => {
  showSuccessModal.value = false
}

const closeErrorModal = () => {
  showErrorModal.value = false
}

onMounted(() => {
  AOS.init({
    duration: 600,
    easing: 'ease-out-cubic',
    once: true,
    offset: 50
  })

  document.title = 'Kontakt | Snap Events'
})
</script>

<style scoped>
.contact-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #FAF8F6 0%, #F5F0EB 100%);
  padding: 60px 24px 80px;
}

.contact-container {
  max-width: 1100px;
  margin: 0 auto;
}

/* Header */
.contact-header {
  text-align: center;
  margin-bottom: 60px;
}

.contact-title {
  font-size: 48px;
  font-weight: 700;
  color: var(--color-text);
  margin: 0 0 16px 0;
}

.contact-subtitle {
  font-size: 18px;
  color: #666;
  margin: 0 0 32px 0;
  max-width: 600px;
  margin-left: auto;
  margin-right: auto;
  line-height: 1.6;
}

.decorative-line {
  width: 80px;
  height: 3px;
  background: linear-gradient(90deg, #C9A88F 0%, #D4AF37 100%);
  margin: 0 auto;
  border-radius: 2px;
}

/* Content Layout */
.contact-content {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: 48px;
  align-items: start;
}

/* Form Wrapper */
.contact-form-wrapper {
  background: white;
  border-radius: 20px;
  padding: 40px;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
}

/* Form Groups */
.form-group {
  margin-bottom: 24px;
}

.form-label {
  display: block;
  font-size: 14px;
  font-weight: 600;
  color: #2D2D2D;
  margin-bottom: 8px;
}

.form-input,
.form-textarea {
  width: 100%;
  padding: 14px 16px;
  font-size: 16px;
  border: 2px solid #E8E8E8;
  border-radius: 12px;
  background: #FAFAFA;
  transition: all 0.3s ease;
  font-family: inherit;
}

.form-input:focus,
.form-textarea:focus {
  outline: none;
  border-color: #C9A88F;
  background: white;
  box-shadow: 0 0 0 4px rgba(201, 168, 143, 0.1);
}

.form-input.error,
.form-textarea.error {
  border-color: #EF4444;
  background: #FEF2F2;
}

.form-input::placeholder,
.form-textarea::placeholder {
  color: #999;
}

.form-textarea {
  resize: vertical;
  min-height: 120px;
}

.form-error {
  display: block;
  font-size: 13px;
  color: #EF4444;
  margin-top: 6px;
}

/* Checkbox */
.checkbox-group {
  margin-top: 32px;
}

.checkbox-label {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  cursor: pointer;
}

.form-checkbox {
  display: none;
}

.checkbox-custom {
  width: 22px;
  height: 22px;
  border: 2px solid #E8E8E8;
  border-radius: 6px;
  background: #FAFAFA;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  margin-top: 2px;
}

.form-checkbox:checked + .checkbox-custom {
  background: linear-gradient(135deg, #C9A88F 0%, #D4AF37 100%);
  border-color: #C9A88F;
}

.form-checkbox:checked + .checkbox-custom::after {
  content: '✓';
  color: white;
  font-size: 14px;
  font-weight: 700;
}

.checkbox-text {
  font-size: 14px;
  color: #666;
  line-height: 1.5;
}

.checkbox-text a {
  color: #C9A88F;
  text-decoration: underline;
}

.checkbox-text a:hover {
  color: #D4AF37;
}

/* Submit Button */
.submit-button {
  width: 100%;
  padding: 16px 32px;
  background: linear-gradient(135deg, #C9A88F 0%, #D4AF37 100%);
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 16px rgba(201, 168, 143, 0.3);
  margin-top: 16px;
}

.submit-button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 24px rgba(201, 168, 143, 0.4);
}

.submit-button:active:not(:disabled) {
  transform: translateY(0);
}

.submit-button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.button-content,
.button-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
}

.spinner {
  width: 20px;
  height: 20px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Contact Info Cards */
.contact-info {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.info-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.06);
  transition: all 0.3s ease;
}

.info-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

.info-icon {
  font-size: 32px;
  margin-bottom: 12px;
}

.info-title {
  font-size: 16px;
  font-weight: 700;
  color: #2D2D2D;
  margin: 0 0 6px 0;
}

.info-text {
  font-size: 14px;
  color: #666;
  margin: 0;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 24px;
}

.modal-content {
  background: white;
  border-radius: 20px;
  padding: 40px;
  max-width: 400px;
  width: 100%;
  text-align: center;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
}

.modal-icon {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 32px;
  font-weight: 700;
  margin: 0 auto 24px;
}

.modal-icon.success {
  background: linear-gradient(135deg, #10B981 0%, #059669 100%);
  color: white;
}

.modal-icon.error {
  background: linear-gradient(135deg, #EF4444 0%, #DC2626 100%);
  color: white;
}

.modal-title {
  font-size: 24px;
  font-weight: 700;
  color: #2D2D2D;
  margin: 0 0 12px 0;
}

.modal-text {
  font-size: 16px;
  color: #666;
  margin: 0 0 32px 0;
  line-height: 1.5;
}

.modal-button {
  padding: 14px 32px;
  background: linear-gradient(135deg, #C9A88F 0%, #D4AF37 100%);
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.modal-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(201, 168, 143, 0.3);
}

/* Modal Transitions */
.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .modal-content,
.modal-leave-to .modal-content {
  transform: scale(0.9);
}

/* Responsive */
@media (max-width: 900px) {
  .contact-content {
    grid-template-columns: 1fr;
  }

  .contact-info {
    flex-direction: row;
    flex-wrap: wrap;
  }

  .info-card {
    flex: 1;
    min-width: 140px;
  }
}

@media (max-width: 768px) {
  .contact-page {
    padding: 40px 16px 60px;
  }

  .contact-title {
    font-size: 36px;
  }

  .contact-subtitle {
    font-size: 16px;
  }

  .contact-form-wrapper {
    padding: 24px;
  }

  .info-card {
    padding: 20px;
  }

  .info-icon {
    font-size: 28px;
  }
}

@media (max-width: 480px) {
  .contact-title {
    font-size: 32px;
  }

  .contact-info {
    flex-direction: column;
  }

  .info-card {
    min-width: unset;
  }

  .modal-content {
    padding: 32px 24px;
  }
}
</style>
