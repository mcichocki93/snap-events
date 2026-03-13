<template>
  <div class="admin-layout">
    <aside class="sidebar">
      <div class="sidebar-logo">
        <span>📸</span>
        <span>SnapEvents Admin</span>
      </div>

      <nav class="sidebar-nav">
        <RouterLink to="/admin/clients" class="nav-item">
          <span class="nav-icon">👥</span>
          Galerie
        </RouterLink>
        <RouterLink to="/admin/clients/new" class="nav-item">
          <span class="nav-icon">➕</span>
          Nowa galeria
        </RouterLink>
      </nav>

      <button class="logout-btn" @click="handleLogout">
        <span>🚪</span> Wyloguj
      </button>
    </aside>

    <main class="admin-content">
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAdminStore } from '../../stores/adminStore'

const router = useRouter()
const adminStore = useAdminStore()

function handleLogout() {
  adminStore.logout()
  router.push('/admin/login')
}
</script>

<style scoped>
.admin-layout {
  display: flex;
  min-height: 100vh;
  background: #f3f4f6;
}

.sidebar {
  width: 240px;
  background: #1a1a2e;
  color: white;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
}

.sidebar-logo {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 24px 20px;
  font-size: 16px;
  font-weight: 700;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}

.sidebar-nav {
  flex: 1;
  padding: 16px 12px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  border-radius: 8px;
  color: rgba(255,255,255,0.7);
  text-decoration: none;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s;
}

.nav-item:hover,
.nav-item.router-link-active {
  background: rgba(255,255,255,0.1);
  color: white;
}

.nav-icon {
  font-size: 16px;
}

.logout-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 16px 12px;
  padding: 10px 12px;
  background: none;
  border: 1px solid rgba(255,255,255,0.2);
  border-radius: 8px;
  color: rgba(255,255,255,0.7);
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
}

.logout-btn:hover {
  background: rgba(255,255,255,0.1);
  color: white;
}

.admin-content {
  flex: 1;
  margin-left: 240px;
  padding: 32px;
  min-height: 100vh;
}
</style>
