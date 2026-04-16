<template>
  <div class="min-h-screen flex items-center justify-center">
    <div class="card bg-base-200 w-full max-w-sm shadow-xl">
      <div class="card-body">
        <h2 class="card-title text-2xl justify-center mb-2">{{ title }}</h2>

        <div v-if="error" role="alert" class="alert alert-error alert-soft">
          <span>{{ error }}</span>
        </div>

        <form @submit.prevent="$emit('submit')">
          <slot />

          <button
            type="submit"
            class="btn btn-primary w-full mt-4"
            :disabled="loading"
          >
            <span v-if="loading" class="loading loading-spinner loading-sm"></span>
            {{ submitLabel }}
          </button>
        </form>

        <div class="text-center mt-3 text-sm">
          <slot name="footer" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  defineProps<{
    title: string
    submitLabel: string
    loading?: boolean
    error?: string
  }>();

  defineEmits<{
    submit: []
  }>();
</script>
