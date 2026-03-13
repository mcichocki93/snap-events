import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'
import Gallery from '../views/Gallery.vue'
import SendPhotos from '../views/SendPhotos.vue'
import Terms from '../views/Terms.vue'
import Privacy from '../views/Privacy.vue'
import FAQ from '../views/FAQ.vue'
import Contact from '../views/Contact.vue'
import AdminLogin from '../views/admin/AdminLogin.vue'
import AdminLayout from '../views/admin/AdminLayout.vue'
import AdminClients from '../views/admin/AdminClients.vue'
import AdminClientForm from '../views/admin/AdminClientForm.vue'

const routes = [
  { path: '/', name: 'Home', component: Home },
  { path: '/gallery/:guid', name: 'Gallery', component: Gallery, props: true },
  { path: '/send/:guid', name: 'SendPhotos', component: SendPhotos, props: true },
  { path: '/regulamin', name: 'Terms', component: Terms },
  { path: '/prywatnosc', name: 'Privacy', component: Privacy },
  { path: '/faq', name: 'FAQ', component: FAQ },
  { path: '/kontakt', name: 'Contact', component: Contact },
  { path: '/admin/login', name: 'AdminLogin', component: AdminLogin, meta: { guestOnly: true } },
  {
    path: '/admin',
    component: AdminLayout,
    meta: { requiresAuth: true },
    children: [
      { path: '', redirect: '/admin/clients' },
      { path: 'clients', name: 'AdminClients', component: AdminClients },
      { path: 'clients/new', name: 'AdminClientNew', component: AdminClientForm },
      { path: 'clients/:guid', name: 'AdminClientEdit', component: AdminClientForm, props: true }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to) => {
  const isLoggedIn = !!localStorage.getItem('admin_token')
  if (to.meta.requiresAuth && !isLoggedIn) return { name: 'AdminLogin' }
  if (to.meta.guestOnly && isLoggedIn) return { path: '/admin/clients' }
})

export default router
