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

const TOKEN_KEY = 'session_token'

function authHeaders(): Record<string, string> {
  const token = localStorage.getItem(TOKEN_KEY)
  return token ? { Authorization: `Bearer ${token}` } : {}
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

export async function logout(): Promise<void> {
  await fetch('/api/sessions', {
    method: 'DELETE',
    headers: authHeaders(),
  })
}

// --- Menu Items ---

export interface MenuItemResponse {
  id: number
  name: string
  price: number
}

export async function getMenuItems(): Promise<MenuItemResponse[]> {
  const res = await fetch('/api/menuitems', { headers: authHeaders() })
  return handleResponse<MenuItemResponse[]>(res)
}

export async function getMenuItem(id: number): Promise<MenuItemResponse & { isActive: boolean }> {
  const res = await fetch(`/api/menuitems/${id}`, { headers: authHeaders() })
  return handleResponse<MenuItemResponse & { isActive: boolean }>(res)
}

export async function createMenuItem(name: string, price: number): Promise<MenuItemResponse> {
  const res = await fetch('/api/menuitems', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ name, price }),
  })
  return handleResponse<MenuItemResponse>(res)
}

export async function updateMenuItem(id: number, name: string, price: number): Promise<MenuItemResponse> {
  const res = await fetch(`/api/menuitems/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ name, price }),
  })
  return handleResponse<MenuItemResponse>(res)
}

export async function deleteMenuItem(id: number): Promise<void> {
  const res = await fetch(`/api/menuitems/${id}`, {
    method: 'DELETE',
    headers: authHeaders(),
  })
  if (!res.ok) throw new Error('Failed to delete menu item')
}

// --- Discounts ---

export interface DiscountResponse {
  id: number
  name: string
  type: string
  amount: number
}

export async function getDiscounts(): Promise<DiscountResponse[]> {
  const res = await fetch('/api/discounts', { headers: authHeaders() })
  return handleResponse<DiscountResponse[]>(res)
}

export async function createDiscount(name: string, type: string, amount: number): Promise<DiscountResponse> {
  const res = await fetch('/api/discounts', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ name, type, amount }),
  })
  return handleResponse<DiscountResponse>(res)
}

export async function updateDiscount(id: number, name: string, type: string, amount: number): Promise<DiscountResponse> {
  const res = await fetch(`/api/discounts/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ name, type, amount }),
  })
  return handleResponse<DiscountResponse>(res)
}

export async function deleteDiscount(id: number): Promise<void> {
  const res = await fetch(`/api/discounts/${id}`, {
    method: 'DELETE',
    headers: authHeaders(),
  })
  if (!res.ok) throw new Error('Failed to delete discount')
}

// --- Taxes ---

export interface TaxResponse {
  id: number
  name: string
  percentage: number
}

export async function getTaxes(): Promise<TaxResponse[]> {
  const res = await fetch('/api/taxes', { headers: authHeaders() })
  return handleResponse<TaxResponse[]>(res)
}

export async function createTax(name: string, percentage: number): Promise<TaxResponse> {
  const res = await fetch('/api/taxes', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ name, percentage }),
  })
  return handleResponse<TaxResponse>(res)
}

export async function updateTax(id: number, name: string, percentage: number): Promise<TaxResponse> {
  const res = await fetch(`/api/taxes/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ name, percentage }),
  })
  return handleResponse<TaxResponse>(res)
}

export async function deleteTax(id: number): Promise<void> {
  const res = await fetch(`/api/taxes/${id}`, {
    method: 'DELETE',
    headers: authHeaders(),
  })
  if (!res.ok) throw new Error('Failed to delete tax')
}

// --- Orders ---

export interface OrderSummary {
  id: number
  createdAt: string
  userId: number
  userFirstName: string
  userLastName: string
  items: { menuItemId: number; menuItemName: string; quantity: number; unitPrice: number }[]
  discounts: { discountId: number; name: string; type: string; amount: number }[]
  taxes: { taxId: number; name: string; percentage: number }[]
}

export async function getOrders(): Promise<OrderSummary[]> {
  const res = await fetch('/api/orders', { headers: authHeaders() })
  return handleResponse<OrderSummary[]>(res)
}

export async function getOrder(id: number): Promise<OrderSummary> {
  const res = await fetch(`/api/orders/${id}`, { headers: authHeaders() })
  return handleResponse<OrderSummary>(res)
}

export async function createOrder(
  items: { menuItemId: number; quantity: number }[],
  discountIds: number[],
  taxIds: number[],
): Promise<OrderSummary> {
  const res = await fetch('/api/orders', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ items, discountIds, taxIds }),
  })
  return handleResponse<OrderSummary>(res)
}

export async function updateOrder(
  id: number,
  items: { menuItemId: number; quantity: number }[],
  discountIds: number[],
  taxIds: number[],
): Promise<OrderSummary> {
  const res = await fetch(`/api/orders/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ items, discountIds, taxIds }),
  })
  return handleResponse<OrderSummary>(res)
}

export async function deleteOrder(id: number): Promise<void> {
  const res = await fetch(`/api/orders/${id}`, {
    method: 'DELETE',
    headers: authHeaders(),
  })
  if (!res.ok) throw new Error('Failed to delete order')
}
