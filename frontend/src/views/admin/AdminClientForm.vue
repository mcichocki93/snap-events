<template>
  <div class="admin-form">
    <div class="page-header">
      <RouterLink to="/admin/clients" class="back-link">← Galerie</RouterLink>
      <h2>{{ isEdit ? 'Edytuj galerię' : 'Nowa galeria' }}</h2>
    </div>

    <div v-if="loadingClient" class="state-msg">Ładowanie...</div>
    <div v-else-if="fetchError" class="state-msg error">{{ fetchError }}</div>

    <form v-else @submit.prevent="handleSubmit" class="form-grid">
      <!-- PERSONAL DATA -->
      <section class="form-section">
        <h3>Dane klienta</h3>
        <div class="form-row">
          <div class="form-group">
            <label>Imię *</label>
            <input v-model="form.firstName" type="text" required />
          </div>
          <div class="form-group">
            <label>Nazwisko *</label>
            <input v-model="form.lastName" type="text" required />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Email *</label>
            <input v-model="form.email" type="email" required />
          </div>
          <div class="form-group">
            <label>Telefon</label>
            <input v-model="form.phone" type="tel" placeholder="+48123456789" />
          </div>
        </div>
      </section>

      <!-- EVENT INFO -->
      <section class="form-section">
        <h3>Wydarzenie</h3>
        <div class="form-row">
          <div class="form-group">
            <label>Nazwa galerii *</label>
            <input v-model="form.eventName" type="text" required placeholder="np. Ślub Kowalskich" />
          </div>
          <div class="form-group">
            <label>Typ wydarzenia *</label>
            <select v-model="form.eventType" required>
              <option value="Wedding">Ślub</option>
              <option value="Birthday">Urodziny</option>
              <option value="Baptism">Chrzciny</option>
              <option value="Communion">Komunia</option>
              <option value="Corporate">Firmowe</option>
              <option value="Conference">Konferencja</option>
              <option value="Other">Inne</option>
            </select>
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Data wydarzenia</label>
            <input v-model="form.eventDate" type="date" />
          </div>
          <div class="form-group">
            <label>Dostępna do *</label>
            <input v-model="form.dateTo" type="date" required />
          </div>
        </div>
      </section>

      <!-- PAKIET -->
      <section class="form-section">
        <h3>Pakiet i limity</h3>
        <div class="form-row">
          <div class="form-group">
            <label>Pakiet</label>
            <select v-model="selectedPackage" @change="applyPackage">
              <option value="starter">Starter (150 zdjęć, 14 dni)</option>
              <option value="standard">Standard (bez limitu, 30 dni)</option>
              <option value="premium">Premium (bez limitu, 90 dni)</option>
              <option value="custom">Własny</option>
            </select>
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Maks. zdjęć <span class="hint">(0 = bez limitu)</span></label>
            <input v-model.number="form.maxFiles" type="number" min="0" max="10000" required />
          </div>
          <div class="form-group">
            <label>Maks. rozmiar pliku (MB)</label>
            <input v-model.number="maxFileSizeMB" type="number" min="1" max="100" required />
          </div>
        </div>
        <div v-if="isEdit" class="form-group">
          <label class="checkbox-label">
            <input v-model="form.isActive" type="checkbox" />
            Galeria aktywna
          </label>
        </div>
      </section>

      <!-- STORAGE -->
      <section class="form-section">
        <h3>Google Drive</h3>
        <div class="form-group">
          <label>URL folderu Google Drive *</label>
          <input v-model="form.googleStorageUrl" type="url" required placeholder="https://drive.google.com/drive/folders/..." />
        </div>
        <div v-if="!isEdit" class="form-group">
          <label>GUID galerii</label>
          <div class="guid-row">
            <input v-model="form.guid" type="text" required placeholder="Wygeneruj lub wpisz własny" />
            <button type="button" class="btn-ghost" @click="generateGuid">Generuj</button>
          </div>
          <span class="hint">Unikalny identyfikator linku do galerii</span>
        </div>
      </section>

      <!-- THEME -->
      <section class="form-section">
        <h3>Wygląd galerii</h3>
        <div class="form-row">
          <div class="form-group">
            <label>Kolor tła (start)</label>
            <div class="color-row">
              <input v-model="form.backgroundColor" type="color" class="color-picker" />
              <input v-model="form.backgroundColor" type="text" class="color-text" />
            </div>
          </div>
          <div class="form-group">
            <label>Kolor tła (koniec)</label>
            <div class="color-row">
              <input v-model="form.backgroundColorSecondary" type="color" class="color-picker" />
              <input v-model="form.backgroundColorSecondary" type="text" class="color-text" />
            </div>
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Kolor tekstu</label>
            <div class="color-row">
              <input v-model="form.fontColor" type="color" class="color-picker" />
              <input v-model="form.fontColor" type="text" class="color-text" />
            </div>
          </div>
          <div class="form-group">
            <label>Kolor akcentu</label>
            <div class="color-row">
              <input v-model="form.accentColor" type="color" class="color-picker" />
              <input v-model="form.accentColor" type="text" class="color-text" />
            </div>
          </div>
        </div>
        <div class="form-group">
          <label>Czcionka</label>
          <select v-model="form.fontType">
            <option value="Roboto">Roboto</option>
            <option value="Playfair Display">Playfair Display</option>
            <option value="Lato">Lato</option>
            <option value="Montserrat">Montserrat</option>
            <option value="Open Sans">Open Sans</option>
            <option value="Raleway">Raleway</option>
          </select>
        </div>

        <!-- Live preview -->
        <div class="theme-preview" :style="previewStyle">
          <span>Podgląd motywu — {{ form.eventName || 'Nazwa galerii' }}</span>
        </div>
      </section>

      <div v-if="submitError" class="submit-error">{{ submitError }}</div>

      <div class="form-actions">
        <RouterLink to="/admin/clients" class="btn-ghost">Anuluj</RouterLink>
        <button type="submit" class="btn-primary" :disabled="saving">
          {{ saving ? 'Zapisywanie...' : isEdit ? 'Zapisz zmiany' : 'Utwórz galerię' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { adminApi } from '../../services/adminApi'

const props = defineProps<{ guid?: string }>()
const router = useRouter()

const isEdit = computed(() => !!props.guid)

const loadingClient = ref(false)
const fetchError = ref('')
const saving = ref(false)
const submitError = ref('')
const selectedPackage = ref('standard')

const form = ref({
  guid: '',
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  eventName: '',
  eventType: 'Wedding',
  eventDate: '',
  dateTo: '',
  isActive: true,
  maxFiles: 0,
  maxFileSize: 10485760,
  backgroundColor: '#667eea',
  backgroundColorSecondary: '#764ba2',
  fontColor: '#ffffff',
  fontType: 'Roboto',
  accentColor: '#3b82f6',
  googleStorageUrl: ''
})

const maxFileSizeMB = computed({
  get: () => Math.round(form.value.maxFileSize / (1024 * 1024)),
  set: (mb: number) => { form.value.maxFileSize = mb * 1024 * 1024 }
})

const previewStyle = computed(() => ({
  background: `linear-gradient(135deg, ${form.value.backgroundColor} 0%, ${form.value.backgroundColorSecondary} 100%)`,
  color: form.value.fontColor,
  fontFamily: form.value.fontType
}))

function applyPackage() {
  const today = new Date()
  const addDays = (d: number) => {
    const dt = new Date(today)
    dt.setDate(dt.getDate() + d)
    return dt.toISOString().split('T')[0]
  }

  if (selectedPackage.value === 'starter') {
    form.value.maxFiles = 150
    form.value.dateTo = addDays(14)
  } else if (selectedPackage.value === 'standard') {
    form.value.maxFiles = 0
    form.value.dateTo = addDays(30)
  } else if (selectedPackage.value === 'premium') {
    form.value.maxFiles = 0
    form.value.dateTo = addDays(90)
  }
}

function generateGuid() {
  form.value.guid = crypto.randomUUID()
}

async function handleSubmit() {
  saving.value = true
  submitError.value = ''
  try {
    if (isEdit.value && props.guid) {
      await adminApi.updateClient(props.guid, {
        firstName: form.value.firstName,
        lastName: form.value.lastName,
        email: form.value.email,
        phone: form.value.phone || undefined,
        eventName: form.value.eventName,
        eventType: form.value.eventType,
        eventDate: form.value.eventDate || undefined,
        dateTo: new Date(form.value.dateTo).toISOString(),
        isActive: form.value.isActive,
        maxFiles: form.value.maxFiles,
        maxFileSize: form.value.maxFileSize,
        backgroundColor: form.value.backgroundColor,
        backgroundColorSecondary: form.value.backgroundColorSecondary,
        fontColor: form.value.fontColor,
        fontType: form.value.fontType,
        accentColor: form.value.accentColor,
        googleStorageUrl: form.value.googleStorageUrl
      })
    } else {
      await adminApi.createClient({
        guid: form.value.guid,
        firstName: form.value.firstName,
        lastName: form.value.lastName,
        email: form.value.email,
        phone: form.value.phone || undefined,
        eventName: form.value.eventName,
        eventType: form.value.eventType,
        eventDate: form.value.eventDate || undefined,
        dateTo: new Date(form.value.dateTo).toISOString(),
        maxFiles: form.value.maxFiles,
        maxFileSize: form.value.maxFileSize,
        backgroundColor: form.value.backgroundColor,
        backgroundColorSecondary: form.value.backgroundColorSecondary,
        fontColor: form.value.fontColor,
        fontType: form.value.fontType,
        accentColor: form.value.accentColor,
        googleStorageUrl: form.value.googleStorageUrl
      })
    }
    router.push('/admin/clients')
  } catch (e: any) {
    submitError.value = e.response?.data?.error ?? 'Błąd zapisu — sprawdź dane'
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  if (isEdit.value && props.guid) {
    loadingClient.value = true
    try {
      const client = await adminApi.getClient(props.guid)
      form.value = {
        guid: client.guid,
        firstName: client.firstName,
        lastName: client.lastName,
        email: client.email,
        phone: client.phone ?? '',
        eventName: client.eventName,
        eventType: client.eventType,
        eventDate: client.eventDate ? client.eventDate.split('T')[0] : '',
        dateTo: client.dateTo.split('T')[0],
        isActive: client.isActive,
        maxFiles: client.maxFiles,
        maxFileSize: client.maxFileSize,
        backgroundColor: client.backgroundColor,
        backgroundColorSecondary: client.backgroundColorSecondary,
        fontColor: client.fontColor,
        fontType: client.fontType,
        accentColor: client.accentColor,
        googleStorageUrl: client.googleStorageUrl
      }
    } catch {
      fetchError.value = 'Nie udało się załadować galerii'
    } finally {
      loadingClient.value = false
    }
  } else {
    // Default for new client: standard package
    applyPackage()
    generateGuid()
  }
})
</script>

<style scoped>
.admin-form { max-width: 860px; }

.page-header { margin-bottom: 24px; }
.back-link { color: #6b7280; text-decoration: none; font-size: 14px; }
.back-link:hover { color: #111827; }
.page-header h2 { font-size: 24px; font-weight: 700; color: #111827; margin: 8px 0 0; }

.state-msg { color: #6b7280; padding: 48px; text-align: center; }
.state-msg.error { color: #dc2626; }

.form-grid { display: flex; flex-direction: column; gap: 24px; }

.form-section {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.08);
}

.form-section h3 {
  font-size: 15px;
  font-weight: 700;
  color: #111827;
  margin: 0 0 20px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e5e7eb;
}

.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }

.form-group { display: flex; flex-direction: column; gap: 6px; }

.form-group label {
  font-size: 13px;
  font-weight: 600;
  color: #374151;
}

.form-group input,
.form-group select {
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
}

.form-group input:focus,
.form-group select:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.hint { font-size: 12px; color: #9ca3af; margin-left: 4px; }

.guid-row { display: flex; gap: 8px; }
.guid-row input { flex: 1; }

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  color: #374151;
}

.color-row { display: flex; gap: 8px; align-items: center; }
.color-picker { width: 44px; height: 38px; padding: 2px; border: 1px solid #d1d5db; border-radius: 8px; cursor: pointer; }
.color-text { flex: 1; }

.theme-preview {
  margin-top: 16px;
  padding: 20px;
  border-radius: 10px;
  font-weight: 600;
  font-size: 15px;
  text-align: center;
  transition: all 0.3s;
}

.submit-error {
  background: #fee2e2;
  color: #dc2626;
  border-radius: 8px;
  padding: 12px 16px;
  font-size: 14px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding-bottom: 32px;
}

.btn-primary {
  background: #1a1a2e;
  color: white;
  padding: 11px 24px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  text-decoration: none;
  transition: opacity 0.2s;
}
.btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }

.btn-ghost {
  background: white;
  color: #374151;
  padding: 11px 24px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  text-decoration: none;
}
.btn-ghost:hover { background: #f9fafb; }
</style>
