import { createRouter, createWebHistory } from 'vue-router'
import LoginView from './views/LoginView.vue'
import RegisterView from './views/RegisterView.vue'
import HomeView from './views/HomeView.vue'
import { useAuth } from './composables/useAuth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: HomeView, meta: { requiresAuth: true } },
    { path: '/login', component: LoginView },
    { path: '/register', component: RegisterView },
  ],
})

let initPromise: Promise<void> | null = null

router.beforeEach(async (to) => {
  const auth = useAuth()

  if (!initPromise) {
    initPromise = auth.init()
  }
  await initPromise

  if (to.meta.requiresAuth && !auth.isLoggedIn.value) {
    return '/login'
  }
  if ((to.path === '/login' || to.path === '/register') && auth.isLoggedIn.value) {
    return '/'
  }
})

export default router
