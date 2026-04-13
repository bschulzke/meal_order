<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuth } from '../composables/useAuth'

const router = useRouter()
const { user, logout } = useAuth()

async function onLogout() {
  await logout()
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen flex flex-col items-center justify-center gap-4">
    <h1 class="text-3xl font-bold">Hello, {{ user?.firstName }} {{ user?.lastName }} ({{ user?.username }})</h1>
    <button class="btn btn-outline" @click="onLogout">Log out</button>
  </div>
</template>
