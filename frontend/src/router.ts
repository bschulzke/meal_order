import { createRouter, createWebHistory } from 'vue-router';
import LoginView from './views/LoginView.vue';
import RegisterView from './views/RegisterView.vue';
import PlaceOrderView from './views/PlaceOrderView.vue';
import OrdersView from './views/OrdersView.vue';
import MenuItemsView from './views/MenuItemsView.vue';
import DiscountsView from './views/DiscountsView.vue';
import TaxesView from './views/TaxesView.vue';
import { useAuth } from './composables/useAuth';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/orders/new' },
    { path: '/orders/new', component: PlaceOrderView, meta: { requiresAuth: true } },
    { path: '/orders', component: OrdersView, meta: { requiresAuth: true } },
    { path: '/menu-items', component: MenuItemsView, meta: { requiresAuth: true } },
    { path: '/discounts', component: DiscountsView, meta: { requiresAuth: true } },
    { path: '/taxes', component: TaxesView, meta: { requiresAuth: true } },
    { path: '/login', component: LoginView },
    { path: '/register', component: RegisterView },
  ],
})

let initPromise: Promise<void> | null = null;

router.beforeEach(async (to) => {
  const auth = useAuth();

  if (!initPromise) {
    initPromise = auth.init();
  }
  await initPromise;

  if (to.meta.requiresAuth && !auth.isLoggedIn.value) {
    return '/login';
  }
  if ((to.path === '/login' || to.path === '/register') && auth.isLoggedIn.value) {
    return '/';
  }
})

export default router;
