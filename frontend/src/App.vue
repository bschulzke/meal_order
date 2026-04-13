<script setup lang="ts">
import { onMounted } from 'vue'
import { useAuth } from './composables/useAuth'

const { initializing, init } = useAuth()

onMounted(() => init())
</script>

<template>
  <div v-if="initializing" class="min-h-screen flex items-center justify-center">
    <span class="loading loading-spinner loading-lg"></span>
  </div>
  <RouterView v-else />
</template>
