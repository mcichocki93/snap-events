<template>
  <div class="faq-page">
    <div class="faq-container">
      <div class="faq-header" data-aos="fade-up">
        <h1 class="faq-title">Najczęściej zadawane pytania</h1>
        <p class="faq-subtitle">Znajdź odpowiedzi na pytania dotyczące Snap Events</p>
      </div>

      <div class="faq-content">
        <div class="faq-categories">
          <button
            v-for="category in categories"
            :key="category.id"
            :class="['category-button', { active: activeCategory === category.id }]"
            @click="activeCategory = category.id"
            data-aos="fade-up"
          >
            {{ category.name }}
          </button>
        </div>

        <div class="faq-items">
          <div
            v-for="(item, index) in filteredFAQs"
            :key="index"
            class="faq-item"
            data-aos="fade-up"
            :data-aos-delay="index * 50"
          >
            <button
              class="faq-question"
              @click="toggleItem(index)"
              :aria-expanded="openItems.includes(index)"
            >
              <span>{{ item.question }}</span>
              <svg
                class="faq-icon"
                :class="{ open: openItems.includes(index) }"
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
              >
                <path
                  d="M6 9l6 6 6-6"
                  stroke="currentColor"
                  stroke-width="2"
                  stroke-linecap="round"
                  stroke-linejoin="round"
                />
              </svg>
            </button>
            <transition name="accordion">
              <div v-if="openItems.includes(index)" class="faq-answer">
                <p v-html="item.answer"></p>
              </div>
            </transition>
          </div>
        </div>

        <div class="faq-cta" data-aos="fade-up">
          <h3>Nie znalazłeś odpowiedzi?</h3>
          <p>Skontaktuj się z nami, chętnie pomożemy!</p>
          <a href="mailto:kontakt@snapevents.com.pl" class="contact-button">
            <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
              <path d="M2 4h16c1.1 0 2 .9 2 2v10c0 1.1-.9 2-2 2H2c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" stroke="currentColor" stroke-width="2"/>
              <path d="M20 6L10 13 0 6" stroke="currentColor" stroke-width="2"/>
            </svg>
            Napisz do nas
          </a>
        </div>
      </div>

      <div class="faq-footer" data-aos="fade-up">
        <router-link to="/" class="back-button">
          <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
            <path d="M12 16L6 10L12 4" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
          Powrót do strony głównej
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import AOS from 'aos'

const activeCategory = ref('all')
const openItems = ref<number[]>([])

const categories = [
  { id: 'all', name: 'Wszystkie' },
  { id: 'general', name: 'Ogólne' },
  { id: 'pricing', name: 'Cennik i płatności' },
  { id: 'technical', name: 'Techniczne' },
  { id: 'photos', name: 'Zdjęcia' }
]

const faqItems = [
  {
    category: 'general',
    question: 'Czym jest Snap Events?',
    answer: 'Snap Events to prosta i wygodna galeria zdjęć, która pozwala gościom na Twojej uroczystości dodawać zdjęcia bez konieczności instalowania aplikacji. Wystarczy jeden link lub zeskanowanie kodu QR!'
  },
  {
    category: 'general',
    question: 'Czy goście muszą instalować aplikację?',
    answer: 'Nie! To jedna z głównych zalet Snap Events. Goście mogą dodawać zdjęcia bezpośrednio przez przeglądarkę internetową na swoim telefonie lub komputerze. Wystarczy kliknąć link lub zeskanować kod QR.'
  },
  {
    category: 'general',
    question: 'Jakie uroczystości obsługujecie?',
    answer: 'Snap Events jest idealny na każdą uroczystość: wesela, urodziny, chrzciny, komunie, baby shower, wieczory panieńskie/kawalerskie, imprezy firmowe, konferencje i wiele innych!'
  },
  {
    category: 'general',
    question: 'Jak długo trwa przygotowanie galerii?',
    answer: 'Galeria jest gotowa natychmiast po dokonaniu płatności! Otrzymasz email z linkiem do galerii oraz kodem QR w ciągu kilku minut.'
  },
  {
    category: 'pricing',
    question: 'Jakie są różnice między pakietami?',
    answer: '<strong>Starter (99 PLN):</strong> 150 zdjęć, 14 dni dostępności, domyślny motyw<br><strong>Standard (199 PLN):</strong> Bez limitu zdjęć, 30 dni dostępności, domyślny motyw<br><strong>Premium (349 PLN):</strong> Bez limitu zdjęć, 90 dni dostępności, pełna personalizacja (kolory, czcionki, teksty)'
  },
  {
    category: 'pricing',
    question: 'Czy mogę przedłużyć dostępność galerii?',
    answer: 'Tak! Przed upływem czasu dostępności możesz przedłużyć galerię kontaktując się z nami na <a href="mailto:kontakt@snapevents.com.pl">kontakt@snapevents.com.pl</a>. Prześlemy Ci ofertę przedłużenia.'
  },
  {
    category: 'pricing',
    question: 'Jakie formy płatności akceptujecie?',
    answer: 'Akceptujemy wszystkie popularne metody płatności: karty kredytowe/debetowe (Visa, Mastercard), BLIK, przelewy bankowe oraz płatności mobilne. Płatności są obsługiwane przez bezpieczny system płatności.'
  },
  {
    category: 'pricing',
    question: 'Czy mogę anulować zamówienie?',
    answer: 'Tak, masz 14 dni na odstąpienie od umowy bez podania przyczyny. Jeśli galeria nie została jeszcze uruchomiona, zwrócimy pełną kwotę. Szczegóły w <a href="/regulamin">Regulaminie</a>.'
  },
  {
    category: 'technical',
    question: 'Czy zdjęcia są bezpieczne?',
    answer: 'Tak! Wszystkie zdjęcia są przechowywane w zaszyfrowanej formie i automatycznie przesyłane do Twojego Google Drive. Tylko osoby z linkiem mają dostęp do galerii.'
  },
  {
    category: 'technical',
    question: 'Co się dzieje ze zdjęciami po zakończeniu okresu dostępności?',
    answer: 'Wszystkie zdjęcia są już zapisane na Twoim Google Drive, więc masz do nich stały dostęp. Po upływie okresu dostępności galeria online zostaje automatycznie usunięta z naszych serwerów.'
  },
  {
    category: 'technical',
    question: 'Czy mogę pobrać wszystkie zdjęcia jednocześnie?',
    answer: 'Tak! Wszystkie zdjęcia są dostępne na Twoim Google Drive, skąd możesz je łatwo pobrać. Dodatkowo w panelu galerii dostępna jest opcja "Pobierz wszystkie" do pobrania archiwum ZIP.'
  },
  {
    category: 'technical',
    question: 'Czy galeria działa na wszystkich urządzeniach?',
    answer: 'Tak! Snap Events jest w pełni responsywny i działa na wszystkich urządzeniach: smartfonach (iOS i Android), tabletach, laptopach i komputerach stacjonarnych. Wystarczy przeglądarka internetowa.'
  },
  {
    category: 'technical',
    question: 'Czy potrzebuję konta Google?',
    answer: 'Jako organizator potrzebujesz konta Google, aby zdjęcia mogły być automatycznie przesyłane do Twojego Google Drive. Goście dodający zdjęcia nie potrzebują żadnego konta.'
  },
  {
    category: 'photos',
    question: 'Czy jest limit rozmiaru zdjęć?',
    answer: 'Pojedyncze zdjęcie może mieć maksymalnie 20 MB. Akceptujemy najpopularniejsze formaty: JPG, JPEG, PNG, HEIC. Zdjęcia są automatycznie optymalizowane.'
  },
  {
    category: 'photos',
    question: 'Czy mogę usunąć niechciane zdjęcia?',
    answer: 'Tak! Jako organizator masz pełną kontrolę nad galerią. Możesz usuwać pojedyncze zdjęcia, które nie powinny się tam znaleźć.'
  },
  {
    category: 'photos',
    question: 'Czy goście mogą dodawać opisy do zdjęć?',
    answer: 'Obecnie goście mogą dodawać tylko zdjęcia. Planujemy dodać możliwość opisów i komentarzy w przyszłości!'
  },
  {
    category: 'photos',
    question: 'Co z jakością zdjęć? Czy są kompresowane?',
    answer: 'Zdjęcia są lekko optymalizowane do wyświetlania w galerii, ale oryginalne pliki w pełnej jakości są zawsze zapisywane na Twoim Google Drive.'
  },
  {
    category: 'technical',
    question: 'Czy mogę zmienić kod QR po utworzeniu galerii?',
    answer: 'Link i kod QR są stałe i nie można ich zmienić. Możesz jednak utworzyć nową galerię, jeśli jest taka potrzeba.'
  },
  {
    category: 'general',
    question: 'Czy mogę mieć kilka galerii jednocześnie?',
    answer: 'Tak! Możesz utworzyć dowolną liczbę galerii dla różnych wydarzeń. Każda galeria ma swój unikalny link i kod QR.'
  },
  {
    category: 'technical',
    question: 'Co zrobić jeśli zapomniałem linku do galerii?',
    answer: 'Link do galerii został wysłany na Twój adres email przy zakupie. Sprawdź skrzynkę odbiorczą i folder spam. Jeśli nie możesz znaleźć emaila, skontaktuj się z nami: <a href="mailto:kontakt@snapevents.com.pl">kontakt@snapevents.com.pl</a>'
  },
  {
    category: 'pricing',
    question: 'Czy są jakieś ukryte koszty?',
    answer: 'Nie! Cena jest jednorazowa i obejmuje wszystkie funkcje wymienione w pakiecie. Nie ma żadnych dodatkowych opłat za przechowywanie czy liczbę gości.'
  }
]

const filteredFAQs = computed(() => {
  if (activeCategory.value === 'all') {
    return faqItems
  }
  return faqItems.filter(item => item.category === activeCategory.value)
})

const toggleItem = (index: number) => {
  const idx = openItems.value.indexOf(index)
  if (idx > -1) {
    openItems.value.splice(idx, 1)
  } else {
    openItems.value.push(index)
  }
}

onMounted(() => {
  AOS.init({
    duration: 600,
    easing: 'ease-out-cubic',
    once: true,
    offset: 50
  })

  document.title = 'FAQ - Snap Events'

  const metaDescription = document.querySelector('meta[name="description"]')
  if (metaDescription) {
    metaDescription.setAttribute('content', 'Najczęściej zadawane pytania dotyczące Snap Events - galerii zdjęć na wesela, urodziny i inne uroczystości.')
  }
})
</script>

<style scoped>
.faq-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #FAF8F6 0%, #FFFFFF 100%);
  padding: 40px 24px 80px;
}

.faq-container {
  max-width: 900px;
  margin: 0 auto;
}

.faq-header {
  text-align: center;
  margin-bottom: 60px;
}

.faq-title {
  font-size: 48px;
  font-weight: 700;
  margin: 0 0 16px 0;
  color: var(--color-text);
  background: linear-gradient(135deg, #C9A88F 0%, #D4AF37 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.faq-subtitle {
  font-size: 20px;
  color: #6B6B6B;
  margin: 0;
  font-weight: 500;
}

.faq-content {
  background: white;
  border-radius: 24px;
  padding: 48px;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
  margin-bottom: 32px;
}

/* Categories */
.faq-categories {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 40px;
  padding-bottom: 32px;
  border-bottom: 2px solid #F5F5F5;
}

.category-button {
  padding: 10px 20px;
  background: #F5F5F5;
  border: 2px solid transparent;
  border-radius: 24px;
  font-size: 15px;
  font-weight: 500;
  color: #6B6B6B;
  cursor: pointer;
  transition: all 0.3s ease;
}

.category-button:hover {
  background: #E8D5C4;
  color: #2D2D2D;
}

.category-button.active {
  background: linear-gradient(135deg, #C9A88F 0%, #D4AF37 100%);
  color: white;
  border-color: transparent;
}

/* FAQ Items */
.faq-items {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.faq-item {
  border: 2px solid #F5F5F5;
  border-radius: 16px;
  overflow: hidden;
  transition: all 0.3s ease;
}

.faq-item:hover {
  border-color: #E8D5C4;
  box-shadow: 0 2px 12px rgba(201, 168, 143, 0.1);
}

.faq-question {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  background: white;
  border: none;
  text-align: left;
  font-size: 17px;
  font-weight: 600;
  color: #2D2D2D;
  cursor: pointer;
  transition: all 0.3s ease;
}

.faq-question:hover {
  color: #C9A88F;
}

.faq-icon {
  flex-shrink: 0;
  color: #C9A88F;
  transition: transform 0.3s ease;
}

.faq-icon.open {
  transform: rotate(180deg);
}

.faq-answer {
  padding: 0 24px 24px 24px;
  background: #FAF8F6;
}

.faq-answer p {
  font-size: 16px;
  line-height: 1.8;
  color: #2D2D2D;
  margin: 0;
}

.faq-answer :deep(a) {
  color: #C9A88F;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.3s ease;
}

.faq-answer :deep(a:hover) {
  color: #B89775;
  text-decoration: underline;
}

.faq-answer :deep(strong) {
  color: #2D2D2D;
  font-weight: 600;
  display: block;
  margin-top: 8px;
}

.faq-answer :deep(br) {
  display: block;
  content: "";
  margin: 8px 0;
}

/* Accordion Animation */
.accordion-enter-active,
.accordion-leave-active {
  transition: all 0.3s ease;
  overflow: hidden;
}

.accordion-enter-from,
.accordion-leave-to {
  max-height: 0;
  opacity: 0;
  padding-top: 0;
  padding-bottom: 0;
}

.accordion-enter-to,
.accordion-leave-from {
  max-height: 500px;
  opacity: 1;
}

/* CTA */
.faq-cta {
  margin-top: 48px;
  padding: 40px;
  background: linear-gradient(135deg, #C9A88F 0%, #D4AF37 100%);
  border-radius: 16px;
  text-align: center;
  color: white;
}

.faq-cta h3 {
  font-size: 28px;
  font-weight: 700;
  margin: 0 0 12px 0;
}

.faq-cta p {
  font-size: 18px;
  margin: 0 0 24px 0;
  opacity: 0.95;
}

.contact-button {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 14px 28px;
  background: white;
  color: #C9A88F;
  border: 2px solid white;
  border-radius: 12px;
  text-decoration: none;
  font-size: 16px;
  font-weight: 600;
  transition: all 0.3s ease;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.contact-button:hover {
  background: transparent;
  color: white;
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
}

/* Footer */
.faq-footer {
  text-align: center;
}

.back-button {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 14px 28px;
  background: white;
  color: #C9A88F;
  border: 2px solid #C9A88F;
  border-radius: 12px;
  text-decoration: none;
  font-size: 16px;
  font-weight: 600;
  transition: all 0.3s ease;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
}

.back-button:hover {
  background: #C9A88F;
  color: white;
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(201, 168, 143, 0.3);
}

.back-button svg {
  transition: transform 0.3s ease;
}

.back-button:hover svg {
  transform: translateX(-4px);
}

/* Responsive */
@media (max-width: 768px) {
  .faq-title {
    font-size: 36px;
  }

  .faq-subtitle {
    font-size: 18px;
  }

  .faq-content {
    padding: 32px 24px;
  }

  .faq-categories {
    justify-content: center;
  }

  .category-button {
    font-size: 14px;
    padding: 8px 16px;
  }

  .faq-question {
    font-size: 16px;
    padding: 16px 20px;
  }

  .faq-answer {
    padding: 0 20px 20px 20px;
  }

  .faq-answer p {
    font-size: 15px;
  }

  .faq-cta {
    padding: 32px 24px;
  }

  .faq-cta h3 {
    font-size: 24px;
  }

  .faq-cta p {
    font-size: 16px;
  }
}

@media (max-width: 480px) {
  .faq-page {
    padding: 24px 16px 60px;
  }

  .faq-title {
    font-size: 28px;
  }

  .faq-content {
    padding: 24px 20px;
  }

  .faq-question {
    font-size: 15px;
    padding: 14px 16px;
  }

  .faq-cta {
    padding: 24px 20px;
  }
}
</style>
