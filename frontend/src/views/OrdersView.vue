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
            <th>Date/Time</th>
            <th>User</th>
            <th>Subtotal</th>
            <th>Discount</th>
            <th>Pre-Tax Total</th>
            <th>Tax</th>
            <th>Total</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="order in orders" :key="order.id">
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
  import { ref, onMounted } from 'vue';
  import * as api from '../api';
  import { Order } from '../models/Order';

  const orders = ref<Order[]>([]);
  const loading = ref(false);
  const error = ref('');

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