<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">Orders</h1>

    <div v-if="error" class="alert alert-error mb-4">
      <span>{{ error }}</span>
    </div>

    <div v-if="loading" class="flex justify-center">
      <span class="loading loading-spinner loading-lg"></span>
    </div>

    <div v-else-if="orders.length === 0" class="text-center text-base-content/60">
      No orders found.
    </div>

    <div v-else class="overflow-x-auto">
      <table class="table table-zebra w-full">
        <thead>
          <tr>
            <th @click="sortBy('createdAt')" class="cursor-pointer select-none">
              Date/Time
              <span v-if="sortKey === 'createdAt'">{{ sortOrder === 'asc' ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('user')" class="cursor-pointer select-none">
              User
              <span v-if="sortKey === 'user'">{{ sortOrder === 'asc' ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('subtotal')" class="cursor-pointer select-none">
              Subtotal
              <span v-if="sortKey === 'subtotal'">{{ sortOrder === 'asc' ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('discount')" class="cursor-pointer select-none">
              Discount
              <span v-if="sortKey === 'discount'">{{ sortOrder === 'asc' ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('preTaxTotal')" class="cursor-pointer select-none">
              Pre-Tax Total
              <span v-if="sortKey === 'preTaxTotal'">{{ sortOrder === 'asc' ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('tax')" class="cursor-pointer select-none">
              Tax
              <span v-if="sortKey === 'tax'">{{ sortOrder === 'asc' ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('total')" class="cursor-pointer select-none">
              Total
              <span v-if="sortKey === 'total'">{{ sortOrder === 'asc' ? '▲' : '▼' }}</span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="order in sortedOrders" :key="order.id">
            <td>{{ new Date(order.createdAt).toLocaleString() }}</td>
            <td>{{ `${order.userFirstName} ${order.userLastName}`.trim() }}</td>
            <td>${{ order.subtotal().toFixed(2) }}</td>
            <td>${{ order.totalDiscounts().toFixed(2) }}</td>
            <td>${{ order.preTaxTotal().toFixed(2) }}</td>
            <td>${{ order.totalTaxes().toFixed(2) }}</td>
            <td class="font-bold">${{ order.total().toFixed(2) }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import * as api from '../api';
import { Order } from '../models/Order';

const orders = ref<Order[]>([]);
const loading = ref(false);
const error = ref('');

type SortKey =
  | 'createdAt'
  | 'user'
  | 'subtotal'
  | 'discount'
  | 'preTaxTotal'
  | 'tax'
  | 'total';

const sortKey = ref<SortKey>('createdAt');
const sortOrder = ref<'asc' | 'desc'>('desc');

function sortBy(key: SortKey) {
  if (sortKey.value === key) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc';
  } else {
    sortKey.value = key;
    sortOrder.value = key === 'createdAt' ? 'desc' : 'asc';
  }
}

const sortAccessors: Record<SortKey, (o: Order) => number | string> = {
  createdAt:   o => new Date(o.createdAt).getTime(),
  user:        o => `${o.userFirstName} ${o.userLastName}`.toLowerCase(),
  subtotal:    o => o.subtotal(),
  discount:    o => o.totalDiscounts(),
  preTaxTotal: o => o.preTaxTotal(),
  tax:         o => o.totalTaxes(),
  total:       o => o.total(),
};

const sortedOrders = computed(() => {
  const accessor = sortAccessors[sortKey.value];
  const dir = sortOrder.value === 'asc' ? 1 : -1;

  return orders.value
    .map(o => ({ o, k: accessor(o) }))
    .sort((a, b) => (a.k < b.k ? -1 : a.k > b.k ? 1 : 0) * dir)
    .map(({ o }) => o);
});

async function loadOrders() {
  loading.value = true;
  error.value = '';
  try {
    const summaries = await api.getOrders();
    orders.value = summaries.map(s => new Order(
      s.id,
      s.userId,
      s.userFirstName,
      s.userLastName,
      s.createdAt,
      s.items,
      s.discounts,
      s.taxes
    ))
  } catch (e: any) {
    error.value = e.message;
  } finally {
    loading.value = false;
  }
}

onMounted(loadOrders);
</script>