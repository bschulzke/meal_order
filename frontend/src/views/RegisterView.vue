<template>
  <AuthForm
    title="Register"
    submit-label="Register"
    :loading="loading"
    :error="error"
    @submit="onSubmit"
  >
    <FormInput
      v-model="firstName"
      label="First Name"
      placeholder="Jane"
      :error="firstNameError"
    />
    <FormInput
      v-model="lastName"
      label="Last Name"
      placeholder="Smith"
      :error="lastNameError"
    />
    <FormInput
      v-model="username"
      label="Username"
      placeholder="1234"
      :error="usernameError"
    />
    <FormInput
      v-model="password"
      label="Password"
      type="password"
      placeholder="••••"
      :error="passwordError"
    />
    <FormInput
      v-model="confirmPassword"
      label="Confirm Password"
      type="password"
      placeholder="••••"
      :error="confirmPasswordError"
    />

    <template #footer>
      Already have an account?
      <RouterLink to="/login" class="link link-primary">Log in</RouterLink>
    </template>
  </AuthForm>
</template>

<script setup lang="ts">
  import { ref } from 'vue'
  import { useRouter } from 'vue-router'
  import { useAuth } from '../composables/useAuth'
  import AuthForm from '../components/AuthForm.vue'
  import FormInput from '../components/FormInput.vue'

  const router = useRouter()
  const { register } = useAuth()

  const username = ref('')
  const password = ref('')
  const confirmPassword = ref('')
  const firstName = ref('')
  const lastName = ref('')
  const usernameError = ref('')
  const passwordError = ref('')
  const confirmPasswordError = ref('')
  const firstNameError = ref('')
  const lastNameError = ref('')
  const error = ref('')
  const loading = ref(false)

  function validate(): boolean {
    usernameError.value = '';
    passwordError.value = '';
    confirmPasswordError.value = '';
    firstNameError.value = '';
    lastNameError.value = '';

    if (!firstName.value.trim() || firstName.value.trim().length > 25) {
      firstNameError.value = 'First name must be 1-25 characters';
    }
    if (!lastName.value.trim() || lastName.value.trim().length > 25) {
      lastNameError.value = 'Last name must be 1-25 characters';
    }
    if (!/^\d{4}$/.test(username.value)) {
      usernameError.value = 'Username must be exactly 4 digits';
    }
    if (password.value.length < 4 || password.value.length > 16) {
      passwordError.value = 'Password must be 4-16 characters';
    }
    if (password.value !== confirmPassword.value) {
      confirmPasswordError.value = 'Passwords do not match';
    }

    return !firstNameError.value && !lastNameError.value && !usernameError.value && !passwordError.value && !confirmPasswordError.value;
  }

  async function onSubmit() {
    error.value = '';
    if (!validate()) return;

    loading.value = true;
    try {
      await register(username.value, password.value, firstName.value.trim(), lastName.value.trim());
      router.push('/')
    } catch (e: any) {
      error.value = e.message;
    } finally {
      loading.value = false;
    }
  }
</script>
