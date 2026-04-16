<template>
  <fieldset class="fieldset">
    <legend class="fieldset-legend">{{ label }}</legend>
    <input
      :type="type ?? 'text'"
      :value="modelValue"
      :placeholder="placeholder"
      class="input w-full"
      :class="{ 'input-error': error }"
      @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
    />
    <p v-if="error" class="fieldset-label text-error">{{ error }}</p>
  </fieldset>
</template>

<script setup lang="ts">
  defineProps<{
    label: string
    type?: string
    modelValue: string
    placeholder?: string
    error?: string
  }>();

  defineEmits<{
    'update:modelValue': [value: string]
  }>();
</script>