<template>
  <div class="navbar bg-base-200 px-4">
    <div class="flex-1 flex gap-1">
      <RouterLink to="/orders/new" :class="navClass('/orders/new')">Place Order</RouterLink>
      <RouterLink to="/orders" :class="navClass('/orders')">Orders</RouterLink>
      <RouterLink to="/menu-items" :class="navClass('/menu-items')">Menu Items</RouterLink>
      <RouterLink to="/discounts" :class="navClass('/discounts')">Discounts</RouterLink>
      <RouterLink to="/taxes" :class="navClass('/taxes')">Taxes</RouterLink>
    </div>
    <div class="flex-none flex items-center gap-3">
      <span class="text-sm">{{ user?.firstName }} {{ user?.lastName }}</span>
      <button class="btn btn-ghost btn-sm" @click="onLogout">Log out</button>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { useRouter, useRoute } from 'vue-router';
  import { useAuth } from '../composables/useAuth';

  const router = useRouter();
  const route = useRoute();
  const { user, logout } = useAuth();

  function navClass(path: string) {
    return route.path === path ? 'btn btn-ghost btn-sm btn-active' : 'btn btn-ghost btn-sm';
  }

  async function onLogout() {
    await logout();
    router.push('/login');
  }
</script>
