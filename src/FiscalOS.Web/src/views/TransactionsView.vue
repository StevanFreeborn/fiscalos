<script setup lang="ts">
import { useTransactionService } from '@/composables/useTransactionService';
import type { Transaction } from '@/services/transactionService';
import { useUserStore } from '@/stores/userStore';
import { onMounted, ref } from 'vue';

const userStore = useUserStore();
const transactionService = useTransactionService(userStore);

const transactions = ref<Transaction[]>([]);

onMounted(async () => {
  const result = await transactionService.get();

  if (result.err) {
    alert(result.val.map(e => e.message).join('\n'));
    return;
  }

  transactions.value = result.val.items;
});
</script>

<template>
  <div>
    <h1>Transactions</h1>
    <ul>
      <li v-for="transaction in transactions" :key="transaction.id">
        <div class="card">
          <div class="top">
            <div>
              <div>{{ transaction.merchantName }}</div>
            </div>
            <div>
              <div>{{ transaction.amount }}</div>
            </div>
          </div>
          <div class="bottom">
            <p>{{ transaction.description }}</p>
          </div>
        </div>
      </li>
    </ul>
  </div>
</template>

<style scoped></style>
