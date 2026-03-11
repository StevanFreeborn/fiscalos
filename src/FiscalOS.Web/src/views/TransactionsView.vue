<script setup lang="ts">
  import TransactionCard from '@/components/TransactionCard.vue';
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
    <ul class="transactions">
      <li
        v-for="transaction in transactions"
        :key="transaction.id"
      >
        <TransactionCard :transaction="transaction" />
      </li>
    </ul>
  </div>
</template>

<style scoped>
  .transactions {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    padding: 1rem;
  }
</style>
