import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { adminApi } from '../services/adminApi'

export const useAdminStore = defineStore('admin', () => {
  const token = ref<string | null>(localStorage.getItem('admin_token'))
  const loading = ref(false)
  const error = ref<string | null>(null)

  const isLoggedIn = computed(() => !!token.value)

  async function login(username: string, password: string): Promise<boolean> {
    loading.value = true
    error.value = null
    try {
      const jwt = await adminApi.login(username, password)
      token.value = jwt
      localStorage.setItem('admin_token', jwt)
      return true
    } catch {
      error.value = 'Nieprawidłowy login lub hasło'
      return false
    } finally {
      loading.value = false
    }
  }

  function logout() {
    token.value = null
    localStorage.removeItem('admin_token')
  }

  return { token, loading, error, isLoggedIn, login, logout }
})
