import axios from 'axios'
import type { AdminClient, CreateClientPayload, UpdateClientPayload } from '../types/adminTypes'

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api'

const adminClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' }
})

// Attach JWT token to every request
adminClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('admin_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Redirect to login on 401
adminClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('admin_token')
      window.location.href = '/admin/login'
    }
    return Promise.reject(error)
  }
)

export const adminApi = {
  async login(username: string, password: string): Promise<string> {
    const { data } = await adminClient.post<{ token: string }>('/admin/login', { username, password })
    return data.token
  },

  async getClients(): Promise<AdminClient[]> {
    const { data } = await adminClient.get<AdminClient[]>('/admin/clients')
    return data
  },

  async getClient(guid: string): Promise<AdminClient> {
    const { data } = await adminClient.get<AdminClient>(`/admin/clients/${guid}`)
    return data
  },

  async createClient(payload: CreateClientPayload): Promise<AdminClient> {
    const { data } = await adminClient.post<AdminClient>('/admin/clients', payload)
    return data
  },

  async updateClient(guid: string, payload: UpdateClientPayload): Promise<AdminClient> {
    const { data } = await adminClient.put<AdminClient>(`/admin/clients/${guid}`, payload)
    return data
  },

  async deleteClient(guid: string): Promise<void> {
    await adminClient.delete(`/admin/clients/${guid}`)
  }
}
