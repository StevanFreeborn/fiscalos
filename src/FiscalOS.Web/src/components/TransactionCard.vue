<script setup lang="ts">
  import type { Transaction } from '@/services/transactionService';

  defineProps<{
    transaction: Transaction;
  }>();

  function formatDate(date: string) {
    const formatter = new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });

    return formatter.format(new Date(date));
  }

  function formatAmount(amount: number): string {
    const formatter = new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    });

    return formatter.format(amount);
  }
</script>

<template>
  <div class="card">
    <div class="top">
      <div class="left">
        <div>{{ transaction.merchantName }}</div>
        <div class="date">{{ formatDate(transaction.date) }}</div>
      </div>
      <div class="right">
        <div>{{ formatAmount(transaction.amount) }}</div>
      </div>
    </div>
    <div class="bottom">
      <p>{{ transaction.description }}</p>
    </div>
  </div>
</template>

<style scoped>
  .card {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    background: var(--bg-surface);
    border-radius: 0.25rem;
    padding: 1rem;
  }

  .top {
    display: flex;
    justify-content: space-between;
    gap: 1rem;
  }

  .left {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .date,
  .bottom {
    font-size: 0.875rem;
    color: #666666;
  }
</style>
