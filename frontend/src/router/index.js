import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'
import Gallery from '../views/Gallery.vue'
import SendPhotos from '../views/SendPhotos.vue'
import Terms from '../views/Terms.vue'
import Privacy from '../views/Privacy.vue'
import FAQ from '../views/FAQ.vue'
import Contact from '../views/Contact.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/gallery/:guid',
    name: 'Gallery',
    component: Gallery,
    props: true
  },
  {
    path: '/send/:guid',
    name: 'SendPhotos',
    component: SendPhotos,
    props: true
  },
  {
    path: '/regulamin',
    name: 'Terms',
    component: Terms
  },
  {
    path: '/prywatnosc',
    name: 'Privacy',
    component: Privacy
  },
  {
    path: '/faq',
    name: 'FAQ',
    component: FAQ
  },
  {
    path: '/kontakt',
    name: 'Contact',
    component: Contact
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
