export interface UserResponse {
  id: number
  username: string
  firstName: string
  lastName: string
}

export interface SessionResponse {
  token: string
  expiresAt: string
}

export interface SessionInfo {
  id: number
  username: string
  firstName: string
  lastName: string
}

export interface ApiError {
  message: string
}

async function handleResponse<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const text = await res.text()
    let message: string
    try {
      const json = JSON.parse(text)
      message = json.message ?? json.title ?? text
    } catch {
      message = text
    }
    throw new Error(message)
  }
  return res.json()
}

export async function registerUser(username: string, password: string, firstName: string, lastName: string): Promise<UserResponse> {
  const res = await fetch('/api/users', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password, firstName, lastName }),
  })
  return handleResponse<UserResponse>(res)
}

export async function login(username: string, password: string): Promise<SessionResponse> {
  const res = await fetch('/api/sessions', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  })
  return handleResponse<SessionResponse>(res)
}

export async function getSession(token: string): Promise<SessionInfo> {
  const res = await fetch(`/api/sessions/${encodeURIComponent(token)}`)
  return handleResponse<SessionInfo>(res)
}

export async function logout(token: string): Promise<void> {
  await fetch(`/api/sessions/${encodeURIComponent(token)}`, {
    method: 'DELETE',
  })
}
