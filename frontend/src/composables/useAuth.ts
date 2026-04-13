import { ref, computed } from 'vue'
import * as api from '../api'

const TOKEN_KEY = 'session_token'

const user = ref<{ userId: number; username: string } | null>(null)
const token = ref<string | null>(null)
const initializing = ref(true)

export function useAuth() {
  const isLoggedIn = computed(() => user.value !== null)

  async function init() {
    const stored = localStorage.getItem(TOKEN_KEY)
    if (!stored) {
      initializing.value = false
      return
    }
    try {
      const info = await api.getSession(stored)
      token.value = stored
      user.value = info
    } catch {
      localStorage.removeItem(TOKEN_KEY)
    } finally {
      initializing.value = false
    }
  }

  async function login(username: string, password: string) {
    const session = await api.login(username, password)
    token.value = session.token
    localStorage.setItem(TOKEN_KEY, session.token)
    const info = await api.getSession(session.token)
    user.value = info
  }

  async function register(username: string, password: string) {
    await api.registerUser(username, password)
    await login(username, password)
  }

  async function logout() {
    if (token.value) {
      try {
        await api.logout(token.value)
      } catch {
        // ignore logout errors
      }
    }
    token.value = null
    user.value = null
    localStorage.removeItem(TOKEN_KEY)
  }

  return { user, token, isLoggedIn, initializing, init, login, register, logout }
}
