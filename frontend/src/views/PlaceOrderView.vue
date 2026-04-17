<template>
  <div class="p-6 max-w-5xl mx-auto">
    <h1 class="text-2xl font-bold mb-4">Place Order</h1>
    <div v-if="error" class="alert alert-error mb-4">
      <span>{{ error }}</span>
    </div>
    <div v-if="loading" class="flex justify-center">
      <span class="loading loading-spinner loading-lg"></span>
    </div>
    <div v-else class="flex flex-col md:flex-row gap-8">
      <!-- Left: Form -->
      <form @submit.prevent="submitOrder" class="flex-1 space-y-8">
        <div>
          <h2 class="text-lg font-semibold mb-2">Menu Items</h2>
          <table class="table w-full">
            <thead>
              <tr>
                <th>Name</th>
                <th>Price</th>
                <th>Quantity</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in menuItems" :key="item.id">
                <td>{{ item.name }}</td>
                <td>${{ item.price.toFixed(2) }}</td>
                <td>
                  <input type="number" min="0" :max="99" class="input input-bordered input-sm w-20" v-model.number="selectedItems.find(i => i.menuItemId === item.id)!.quantity"" />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div>
          <h2 class="text-lg font-semibold mb-2">Discounts</h2>
          <div class="flex flex-wrap gap-4">
            <label v-for="d in discounts" :key="d.id" class="flex items-center gap-2">
              <input type="checkbox" :value="d.id" v-model="selectedDiscountIds" />
              <span>{{ d.name }} <span v-if="d.type === 'percent'">({{ d.amount }}%)</span><span v-else>(${{ d.amount.toFixed(2) }})</span></span>
            </label>
          </div>
        </div>
        <div>
          <h2 class="text-lg font-semibold mb-2">Taxes</h2>
          <div class="flex flex-wrap gap-4">
            <label v-for="t in taxes" :key="t.id" class="flex items-center gap-2">
              <input type="checkbox" :value="t.id" v-model="selectedTaxIds" />
              <span>{{ t.name }} ({{ t.percentage }}%)</span>
            </label>
          </div>
        </div>
      </form>
      <!-- Right: Order Summary -->
      <div class="flex-1 max-w-md">
        <div class="card bg-base-100 shadow p-6">
          <h2 class="text-lg font-semibold mb-4">Order Summary</h2>
          <div v-if="itemDetails.length === 0" class="text-base-content/60 mb-2">No items selected.</div>
          <table v-else class="table table-sm w-full mb-2">
            <thead>
              <tr>
                <th>Item</th>
                <th>Qty</th>
                <th>Price</th>
                <th>Total</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in itemDetails" :key="item.menuItemId">
                <td>{{ item.name }}</td>
                <td>{{ item.quantity }}</td>
                <td>${{ item.price.toFixed(2) }}</td>
                <td>${{ item.lineTotal.toFixed(2) }}</td>
              </tr>
            </tbody>
          </table>
          <div class="divider my-2"></div>
          <div class="flex justify-between mb-1"><span>Subtotal</span><span>${{ previewOrder.subtotal().toFixed(2) }}</span></div>
          <div class="flex justify-between mb-1">
            <span>Discounts</span>
            <span v-if="selectedDiscounts.length > 0">- ${{ previewOrder.totalDiscounts().toFixed(2) }}</span>
            <span v-else class="text-base-content/60">No discount</span>
          </div>
          <div class="flex justify-between mb-1">
            <span>Pre-Tax Total</span><span>${{ previewOrder.preTaxTotal().toFixed(2) }}</span>
          </div>
          <div class="flex justify-between mb-1">
            <span>Taxes</span>
            <span v-if="selectedTaxes.length > 0">+ ${{ previewOrder.totalTaxes().toFixed(2) }}</span>
            <span v-else class="text-base-content/60">No taxes</span>
          </div>
          <div class="divider my-2"></div>
          <div class="flex justify-between text-lg font-bold mb-2">
            <span>Total</span><span>${{ previewOrder.total().toFixed(2) }}</span>
          </div>
          <div v-if="submitError" class="alert alert-error mb-4">
            <span>{{ submitError }}</span>
          </div>
          <button class="btn btn-primary w-full" type="button" :disabled="submitting || itemDetails.length === 0" @click="submitOrder">
            <span v-if="submitting" class="loading loading-spinner loading-xs"></span>
            Place Order
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed } from 'vue';
  import * as api from '../api';
  import { Order } from '../models/Order';

  const menuItems = ref<api.MenuItemResponse[]>([]);
  const discounts = ref<api.DiscountResponse[]>([]);
  const taxes = ref<api.TaxResponse[]>([]);

  const selectedItems = ref<{ menuItemId: number; quantity: number }[]>([]);
  const selectedDiscountIds = ref<number[]>([]);
  const selectedTaxIds = ref<number[]>([]);

  const loading = ref(false);
  const error = ref('');
  const submitting = ref(false);
  const submitError = ref('');

  onMounted(async () => {
    loading.value = true;
    error.value = '';
    try {
      const [mi, d, t] = await Promise.all([
        api.getMenuItems(),
        api.getDiscounts(),
        api.getTaxes()
      ])
      menuItems.value = mi;
      discounts.value = d;
      taxes.value = t;
      selectedItems.value = mi.map(m => ({ menuItemId: m.id, quantity: 0 }));
    } catch (e: any) {
      error.value = e.message;
    } finally {
      loading.value = false;
    }
  })

  const itemDetails = computed(() => {
    return selectedItems.value
      .filter(i => i.quantity > 0)
      .map(i => {
        const m = menuItems.value.find(m => m.id === i.menuItemId)
        return m ? { ...i, name: m.name, price: m.price, lineTotal: i.quantity * m.price } : null
      })
      .filter(Boolean) as Array<{ menuItemId: number; quantity: number; name: string; price: number; lineTotal: number }>
  });

  const selectedDiscounts = computed(() => discounts.value.filter(d => selectedDiscountIds.value.includes(d.id)));
  const selectedTaxes = computed(() => taxes.value.filter(t => selectedTaxIds.value.includes(t.id)));

  const previewOrder = computed(() => {
    const items = itemDetails.value.map(i => ({
      menuItemId: i.menuItemId,
      menuItemName: i.name,
      quantity: i.quantity,
      unitPrice: i.price
    }))
    const orderDiscounts = selectedDiscounts.value.map(d => ({
      discountId: d.id,
      name: d.name,
      type: d.type,
      amount: d.amount
    }))
    const orderTaxes = selectedTaxes.value.map(t => ({
      taxId: t.id,
      name: t.name,
      percentage: t.percentage
    }))
    return new Order(
      0, // id
      0, // userId
      '', // userFirstName
      '', // userLastName
      '', // createdAt
      items,
      orderDiscounts,
      orderTaxes
    )
  })

  async function submitOrder() {
    submitError.value = '';
    const items = selectedItems.value.filter(i => i.quantity > 0);
    if (items.length === 0) {
      submitError.value = 'Select at least one menu item with quantity > 0.';
      return;
    }
    submitting.value = true;
    try {
      await api.createOrder(items, selectedDiscountIds.value, selectedTaxIds.value);
      window.location.href = '/orders';
    } catch (e: any) {
      submitError.value = e.message;
    } finally {
      submitting.value = false;
    }
  }
</script>