<template>
  <div class="terms-page">
    <div class="terms-container">
      <div class="terms-header" data-aos="fade-up">
        <h1 class="terms-title">Regulamin</h1>
        <p class="terms-subtitle">Snap Events - Galeria zdjęć na uroczystości</p>
        <p class="terms-date">Ostatnia aktualizacja: 9 stycznia 2026</p>
      </div>

      <div class="terms-content">
        <section class="terms-section" data-aos="fade-up">
          <h2>1. Postanowienia ogólne</h2>
          <p>Niniejszy Regulamin określa zasady korzystania z usługi Snap Events, która umożliwia tworzenie i udostępnianie galerii zdjęć z różnych uroczystości.</p>
          <p>Użytkownikiem może być każda osoba fizyczna, osoba prawna lub jednostka organizacyjna nieposiadająca osobowości prawnej, która ukończyła 18 lat lub działa za zgodą opiekuna prawnego.</p>
        </section>

        <section class="terms-section" data-aos="fade-up">
          <h2>2. Zakres usług</h2>
          <p>Snap Events oferuje następujące usługi:</p>
          <ul>
            <li>Tworzenie dedykowanych galerii zdjęć dla uroczystości</li>
            <li>Możliwość dodawania zdjęć przez gości bez konieczności instalowania aplikacji</li>
            <li>Przechowywanie zdjęć przez określony czas (zależnie od wybranego pakietu)</li>
            <li>Automatyczne przesyłanie zdjęć do Google Drive</li>
            <li>Generowanie unikalnych linków dostępu i kodów QR</li>
          </ul>
        </section>

        <section class="terms-section" data-aos="fade-up">
          <h2>3. Pakiety i płatności</h2>
          <p><strong>Starter (99 PLN):</strong></p>
          <ul>
            <li>Limit 150 zdjęć</li>
            <li>14 dni dostępności galerii</li>
            <li>Domyślny motyw</li>
          </ul>

          <p><strong>Standard (199 PLN):</strong></p>
          <ul>
            <li>Bez limitu zdjęć</li>
            <li>30 dni dostępności galerii</li>
            <li>Domyślny motyw</li>
          </ul>

          <p><strong>Premium (349 PLN):</strong></p>
          <ul>
            <li>Bez limitu zdjęć</li>
            <li>90 dni dostępności galerii</li>
            <li>Pełna personalizacja (kolory, czcionki, teksty)</li>
          </ul>

          <p>Płatność jest jednorazowa i dokonywana przed aktywacją galerii. Po upływie okresu dostępności galeria zostaje automatycznie usunięta z naszych serwerów.</p>
        </section>

        <section class="terms-section" data-aos="fade-up">
          <h2>4. Prawa autorskie i własność zdjęć</h2>
          <p>Wszystkie zdjęcia przesłane do galerii pozostają własnością osób, które je dodały. Organizator uroczystości (właściciel galerii) otrzymuje dostęp do wszystkich zdjęć dodanych przez gości.</p>
          <p>Użytkownik przesyłając zdjęcia oświadcza, że posiada do nich prawa autorskie lub zgodę osób uprawnionych do ich publikacji.</p>
          <p>Snap Events nie rości sobie praw do przesłanych zdjęć i nie wykorzystuje ich w celach komercyjnych.</p>
        </section>

        <section class="terms-section" data-aos="fade-up">
          <h2>5. Ochrona danych osobowych</h2>
          <p>Przetwarzanie danych osobowych odbywa się zgodnie z RODO. Administratorem danych jest Snap Events.</p>
          <p>Dane zbierane przez serwis:</p>
          <ul>
            <li>Adres e-mail organizatora (do wysłania linku do galerii)</li>
            <li>Metadane zdjęć (data, rozmiar)</li>
            <li>Dane techniczne (adres IP, typ przeglądarki) w celach bezpieczeństwa</li>
          </ul>
          <p>Szczegółowe informacje znajdują się w <router-link to="/privacy">Polityce Prywatności</router-link>.</p>
        </section>

        <section class="terms-section" data-aos="fade-up">
          <h2>6. Obowiązki użytkownika</h2>
          <p>Użytkownik zobowiązuje się do:</p>
          <ul>
            <li>Nieprzesyłania treści niezgodnych z prawem, obraźliwych lub naruszających prawa osób trzecich</li>
            <li>Nieprzesyłania zdjęć zawierających wirusy lub inne szkodliwe oprogramowanie</li>
            <li>Nieutrudniania działania serwisu</li>
            <li>Nieudostępniania linku do galerii osobom nieupoważnionym (jeśli galeria ma charakter prywatny)</li>
          </ul>
        </section>

        <section class="terms-section" data-aos="fade-up">
          <h2>7. Odpowiedzialność</h2>
          <p>Snap Events dokłada wszelkich starań, aby usługa działała nieprzerwanie, jednak nie gwarantuje 100% dostępności serwisu.</p>
          <p>Snap Events nie ponosi odpowiedzialności za:</p>
          <ul>
            <li>Utratę danych w wyniku awarii niezależnych od serwisu</li>
            <li>Treści przesyłane przez użytkowników</li>
            <li>Szkody wynikające z niewłaściwego użytkowania serwisu</li>
          </ul>
          <p>Zalecamy regularne tworzenie kopii zapasowych zdjęć.</p>
        </section>

        <section class="terms-section" data-aos="fade-up">
          <h2>8. Prawo odstąpienia od umowy</h2>
          <p>Zgodnie z prawem konsumenckim, użytkownik ma prawo odstąpić od umowy w ciągu 14 dni od daty zakupu bez podania przyczyny.</p>
          <p>W przypadku, gdy usługa zostanie uruchomiona na wyraźne żądanie użytkownika przed upływem 14 dni, prawo odstąpienia wygasa.</p>
          <p>Zwrot środków następuje w ciągu 14 dni od otrzymania oświadczenia o odstąpieniu.</p>
        </section>

        <section class="terms-section" data-aos="fade-up">
          <h2>9. Reklamacje</h2>
          <p>Reklamacje dotyczące usług można zgłaszać na adres e-mail: <a href="mailto:kontakt@snapevents.com.pl">kontakt@snapevents.com.pl</a></p>
          <p>Reklamacja powinna zawierać:</p>
          <ul>
            <li>Opis problemu</li>
            <li>Datę zakupu usługi</li>
            <li>Identyfikator galerii</li>
            <li>Dane kontaktowe</li>
          </ul>
          <p>Reklamacje rozpatrywane są w ciągu 14 dni roboczych.</p>
        </section>

        <section class="terms-section" data-aos="fade-up">
          <h2>10. Zmiany regulaminu</h2>
          <p>Snap Events zastrzega sobie prawo do wprowadzania zmian w Regulaminie. O wszelkich zmianach użytkownicy zostaną poinformowani z 7-dniowym wyprzedzeniem.</p>
          <p>Kontynuowanie korzystania z usługi po wejściu w życie zmian oznacza akceptację nowego regulaminu.</p>
        </section>

        <section class="terms-section" data-aos="fade-up">
          <h2>11. Postanowienia końcowe</h2>
          <p>W sprawach nieuregulowanych niniejszym Regulaminem zastosowanie mają przepisy prawa polskiego.</p>
          <p>Wszelkie spory będą rozstrzygane przez sąd właściwy dla siedziby Snap Events.</p>
        </section>

        <section class="terms-section" data-aos="fade-up">
          <h2>12. Kontakt</h2>
          <p>W razie pytań dotyczących Regulaminu prosimy o kontakt:</p>
          <p>
            <strong>Email:</strong> <a href="mailto:kontakt@snapevents.com.pl">kontakt@snapevents.com.pl</a><br>
            <strong>Strona:</strong> <a href="https://snapevents.pl">www.snapevents.pl</a>
          </p>
        </section>
      </div>

      <div class="terms-footer" data-aos="fade-up">
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
import { onMounted } from 'vue'
import AOS from 'aos'

onMounted(() => {
  AOS.init({
    duration: 600,
    easing: 'ease-out-cubic',
    once: true,
    offset: 50
  })

  document.title = 'Regulamin - Snap Events'

  const metaDescription = document.querySelector('meta[name="description"]')
  if (metaDescription) {
    metaDescription.setAttribute('content', 'Regulamin korzystania z usługi Snap Events - galeria zdjęć na wesela, urodziny i inne uroczystości.')
  }
})
</script>

<style scoped>
.terms-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #FAF8F6 0%, #FFFFFF 100%);
  padding: 40px 24px 80px;
}

.terms-container {
  max-width: 900px;
  margin: 0 auto;
}

.terms-header {
  text-align: center;
  margin-bottom: 60px;
}

.terms-title {
  font-size: 48px;
  font-weight: 700;
  margin: 0 0 16px 0;
  color: var(--color-text);
  background: linear-gradient(135deg, #C9A88F 0%, #D4AF37 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.terms-subtitle {
  font-size: 20px;
  color: #6B6B6B;
  margin: 0 0 8px 0;
  font-weight: 500;
}

.terms-date {
  font-size: 14px;
  color: #999;
  margin: 0;
}

.terms-content {
  background: white;
  border-radius: 24px;
  padding: 48px;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
  margin-bottom: 32px;
}

.terms-section {
  margin-bottom: 40px;
}

.terms-section:last-child {
  margin-bottom: 0;
}

.terms-section h2 {
  font-size: 24px;
  font-weight: 700;
  color: #2D2D2D;
  margin: 0 0 20px 0;
  padding-bottom: 12px;
  border-bottom: 2px solid #E8D5C4;
}

.terms-section p {
  font-size: 16px;
  line-height: 1.8;
  color: #2D2D2D;
  margin: 0 0 16px 0;
}

.terms-section p:last-child {
  margin-bottom: 0;
}

.terms-section ul {
  margin: 16px 0;
  padding-left: 24px;
}

.terms-section li {
  font-size: 16px;
  line-height: 1.8;
  color: #2D2D2D;
  margin-bottom: 8px;
}

.terms-section a {
  color: #C9A88F;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.3s ease;
}

.terms-section a:hover {
  color: #B89775;
  text-decoration: underline;
}

.terms-section strong {
  color: #2D2D2D;
  font-weight: 600;
}

.terms-footer {
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

@media (max-width: 768px) {
  .terms-title {
    font-size: 36px;
  }

  .terms-subtitle {
    font-size: 18px;
  }

  .terms-content {
    padding: 32px 24px;
  }

  .terms-section h2 {
    font-size: 20px;
  }

  .terms-section p,
  .terms-section li {
    font-size: 15px;
  }
}

@media (max-width: 480px) {
  .terms-page {
    padding: 24px 16px 60px;
  }

  .terms-title {
    font-size: 28px;
  }

  .terms-content {
    padding: 24px 20px;
  }
}
</style>
