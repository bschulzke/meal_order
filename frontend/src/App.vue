<script setup lang="ts">
import { useAuth } from './composables/useAuth'
import NavBar from './components/NavBar.vue'

const { initializing, isLoggedIn } = useAuth()
</script>

<template>
  <div v-if="initializing" class="min-h-screen flex items-center justify-center">
    <span class="loading loading-spinner loading-lg"></span>
  </div>
  <template v-else>
    <NavBar v-if="isLoggedIn" />
    <RouterView />
  </template>
</template>
