<script setup lang="ts">
  import CircleSpinner from '@/components/CircleSpinner.vue';
  import type { AvailableAccount } from '@/services/institutionService';
  import { ref, watchEffect } from 'vue';

  const props = defineProps<{
    availableAccounts: AvailableAccount[];
    isSubmitting: boolean;
  }>();

  const emit = defineEmits<{
    submit: [account: AvailableAccount];
  }>();

  const selectedAccount = ref<AvailableAccount | null>(null);
  const selectRef = ref<HTMLSelectElement | null>(null);

  watchEffect(() => {
    if (props.availableAccounts.length > 0) {
      selectRef.value?.focus();
    }
  });

  function handleSubmit() {
    if (selectedAccount.value === null) {
      return;
    }

    emit('submit', selectedAccount.value);
  }
</script>

<template>
  <form
    class="add-account-form"
    @submit.prevent="handleSubmit"
  >
    <label for="account-select">Select account to add:</label>
    <select
      id="account-select"
      ref="selectRef"
      v-model="selectedAccount"
    >
      <option
        :value="null"
        disabled
        hidden
      >
        Select an account
      </option>
      <option
        v-for="account in availableAccounts"
        :key="account.providerId"
        :value="account"
      >
        {{ account.providerName }}
      </option>
    </select>
    <button
      class="add-account-button"
      type="submit"
      :disabled="isSubmitting"
    >
      <CircleSpinner v-if="isSubmitting" />
      <span v-else>Add</span>
    </button>
  </form>
</template>

<style scoped>
  .add-account-form {
    display: flex;
    gap: 0.25rem;
    align-items: center;
  }
</style>
