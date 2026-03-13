<template>
  <div class="admin-clients">
    <div class="page-header">
      <h2>Galerie</h2>
      <RouterLink to="/admin/clients/new" class="btn-primary">+ Nowa galeria</RouterLink>
    </div>

    <div v-if="loading" class="state-msg">Ładowanie...</div>
    <div v-else-if="error" class="state-msg error">{{ error }}</div>

    <div v-else-if="clients.length === 0" class="state-msg">Brak galerii. Utwórz pierwszą!</div>

    <div v-else class="clients-table-wrap">
      <div class="search-bar">
        <input v-model="search" type="text" placeholder="Szukaj po nazwisku, emailu, nazwie galerii..." />
      </div>

      <table class="clients-table">
        <thead>
          <tr>
            <th>Klient</th>
            <th>Galeria</th>
            <th>Pakiet</th>
            <th>Wygasa</th>
            <th>Status</th>
            <th>Zdjęcia</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="client in filteredClients" :key="client.guid">
            <td>
              <div class="client-name">{{ client.firstName }} {{ client.lastName }}</div>
              <div class="client-email">{{ client.email }}</div>
            </td>
            <td>
              <div class="event-name">{{ client.eventTypeEmoji }} {{ client.eventName }}</div>
              <div class="client-email">{{ client.eventTypeDisplayName }}</div>
            </td>
            <td>
              <span class="package-badge" :class="packageClass(client)">
                {{ packageLabel(client) }}
              </span>
            </td>
            <td>
              <div :class="{ 'text-danger': client.isExpired }">
                {{ formatDate(client.dateTo) }}
              </div>
            </td>
            <td>
              <span class="status-badge" :class="client.isActive && !client.isExpired ? 'active' : 'inactive'">
                {{ client.isActive && !client.isExpired ? 'Aktywna' : client.isExpired ? 'Wygasła' : 'Dezaktywowana' }}
              </span>
            </td>
            <td>
              <span v-if="client.maxFiles === 0">{{ client.uploadedFilesCount }} / ∞</span>
              <span v-else>{{ client.uploadedFilesCount }} / {{ client.maxFiles }}</span>
            </td>
            <td class="actions">
              <RouterLink :to="`/admin/clients/${client.guid}`" class="btn-sm">Edytuj</RouterLink>
              <a :href="`/send/${client.guid}`" target="_blank" class="btn-sm btn-ghost">Galeria</a>
              <button class="btn-sm btn-danger" @click="confirmDelete(client)">Usuń</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Delete confirmation modal -->
    <div v-if="clientToDelete" class="modal-overlay" @click.self="clientToDelete = null">
      <div class="modal">
        <h3>Usuń galerię</h3>
        <p>Czy na pewno chcesz usunąć galerię <strong>{{ clientToDelete.eventName }}</strong> ({{ clientToDelete.firstName }} {{ clientToDelete.lastName }})?</p>
        <p class="modal-warning">Tej operacji nie można cofnąć.</p>
        <div class="modal-actions">
          <button class="btn-ghost" @click="clientToDelete = null">Anuluj</button>
          <button class="btn-danger" :disabled="deleting" @click="doDelete">
            {{ deleting ? 'Usuwanie...' : 'Usuń' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { adminApi } from '../../services/adminApi'
import type { AdminClient } from '../../types/adminTypes'

const clients = ref<AdminClient[]>([])
const loading = ref(false)
const error = ref('')
const search = ref('')
const clientToDelete = ref<AdminClient | null>(null)
const deleting = ref(false)

const filteredClients = computed(() => {
  if (!search.value) return clients.value
  const q = search.value.toLowerCase()
  return clients.value.filter(c =>
    c.firstName.toLowerCase().includes(q) ||
    c.lastName.toLowerCase().includes(q) ||
    c.email.toLowerCase().includes(q) ||
    c.eventName.toLowerCase().includes(q)
  )
})

function packageLabel(client: AdminClient): string {
  if (client.maxFiles === 0) return 'Standard/Premium'
  if (client.maxFiles <= 150) return 'Starter'
  return `${client.maxFiles} zdjęć`
}

function packageClass(client: AdminClient): string {
  if (client.maxFiles === 0) return 'pkg-unlimited'
  if (client.maxFiles <= 150) return 'pkg-starter'
  return 'pkg-custom'
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('pl-PL')
}

function confirmDelete(client: AdminClient) {
  clientToDelete.value = client
}

async function doDelete() {
  if (!clientToDelete.value) return
  deleting.value = true
  try {
    await adminApi.deleteClient(clientToDelete.value.guid)
    clients.value = clients.value.filter(c => c.guid !== clientToDelete.value!.guid)
    clientToDelete.value = null
  } catch {
    error.value = 'Błąd podczas usuwania galerii'
  } finally {
    deleting.value = false
  }
}

onMounted(async () => {
  loading.value = true
  try {
    clients.value = await adminApi.getClients()
  } catch {
    error.value = 'Nie udało się załadować galerii'
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.admin-clients { max-width: 1200px; }

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 24px;
}

.page-header h2 { font-size: 24px; font-weight: 700; color: #111827; margin: 0; }

.btn-primary {
  background: #1a1a2e;
  color: white;
  padding: 10px 20px;
  border-radius: 8px;
  text-decoration: none;
  font-size: 14px;
  font-weight: 600;
}

.state-msg { color: #6b7280; font-size: 16px; text-align: center; padding: 48px; }
.state-msg.error { color: #dc2626; }

.search-bar { margin-bottom: 16px; }
.search-bar input {
  width: 100%;
  max-width: 400px;
  padding: 10px 14px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 14px;
  outline: none;
  box-sizing: border-box;
}
.search-bar input:focus { border-color: #3b82f6; }

.clients-table-wrap { background: white; border-radius: 12px; padding: 24px; box-shadow: 0 1px 4px rgba(0,0,0,0.08); }

.clients-table { width: 100%; border-collapse: collapse; font-size: 14px; }
.clients-table th { text-align: left; padding: 10px 12px; color: #6b7280; font-weight: 600; border-bottom: 1px solid #e5e7eb; font-size: 12px; text-transform: uppercase; }
.clients-table td { padding: 14px 12px; border-bottom: 1px solid #f3f4f6; vertical-align: middle; }
.clients-table tr:last-child td { border-bottom: none; }

.client-name { font-weight: 600; color: #111827; }
.client-email { font-size: 12px; color: #6b7280; margin-top: 2px; }
.event-name { font-weight: 500; color: #111827; }
.text-danger { color: #dc2626; }

.status-badge {
  display: inline-block;
  padding: 3px 10px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
}
.status-badge.active { background: #d1fae5; color: #065f46; }
.status-badge.inactive { background: #fee2e2; color: #991b1b; }

.package-badge {
  display: inline-block;
  padding: 3px 10px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
}
.pkg-starter { background: #e0f2fe; color: #0369a1; }
.pkg-unlimited { background: #ede9fe; color: #5b21b6; }
.pkg-custom { background: #fef3c7; color: #92400e; }

.actions { display: flex; gap: 6px; }

.btn-sm {
  padding: 5px 12px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  text-decoration: none;
  border: 1px solid #d1d5db;
  background: white;
  color: #374151;
  transition: all 0.15s;
}
.btn-sm:hover { background: #f9fafb; }
.btn-ghost { background: transparent; }
.btn-danger { background: #fee2e2; border-color: #fca5a5; color: #dc2626; }
.btn-danger:hover { background: #fecaca; }

/* Modal */
.modal-overlay {
  position: fixed; inset: 0;
  background: rgba(0,0,0,0.5);
  display: flex; align-items: center; justify-content: center;
  z-index: 1000;
}
.modal {
  background: white;
  border-radius: 12px;
  padding: 32px;
  max-width: 440px;
  width: 90%;
  box-shadow: 0 20px 60px rgba(0,0,0,0.3);
}
.modal h3 { font-size: 18px; font-weight: 700; color: #111827; margin: 0 0 12px; }
.modal p { color: #374151; margin: 0 0 8px; }
.modal-warning { color: #dc2626; font-size: 13px; }
.modal-actions { display: flex; gap: 10px; justify-content: flex-end; margin-top: 24px; }
</style>
