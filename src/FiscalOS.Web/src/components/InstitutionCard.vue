<script setup lang="ts">
  import AddAccountForm from '@/components/AddAccountForm.vue';
  import CircleSpinner from '@/components/CircleSpinner.vue';
  import { useAccountService } from '@/composables/useAccountService';
  import { useInstitutionService } from '@/composables/useInstitutionService';
  import type { AvailableAccount, Institution } from '@/services/institutionService';
  import { useUserStore } from '@/stores/userStore';
  import { ref } from 'vue';

  const props = defineProps<{
    institution: Institution;
  }>();

  const emit = defineEmits<{
    accountAdded: [];
  }>();

  const userStore = useUserStore();
  const institutionService = useInstitutionService(userStore);
  const accountService = useAccountService(userStore);

  const loading = ref(false);
  const addingAccount = ref(false);
  const availableAccounts = ref<AvailableAccount[]>([]);
  const showAddForm = ref(false);

  async function handleAddAccountClick() {
    loading.value = true;
    const getAccountsResult = await institutionService.getAvailableAccounts(props.institution.id);
    loading.value = false;

    if (getAccountsResult.err) {
      alert('Failed to retrieve accounts to add');
      return;
    }

    const notAlreadyAddedAccounts = getAccountsResult.val.filter(a => {
      return props.institution.accounts.some(ac => ac.providerId === a.providerId) === false;
    });

    availableAccounts.value = notAlreadyAddedAccounts;
    showAddForm.value = true;
  }

  async function handleFormSubmit(account: AvailableAccount) {
    addingAccount.value = true;

    try {
      const addAccountResult = await accountService.add(
        account.providerInstitutionId,
        account.providerId,
        account.providerName
      );

      if (addAccountResult.err) {
        alert(addAccountResult.val.map(e => e.message).join('\n'));
        return;
      }

      showAddForm.value = false;
      emit('accountAdded');
    } finally {
      addingAccount.value = false;
    }
  }
</script>

<template>
  <div class="institution-container">
    <div class="institution-card">
      <div>
        <div>{{ institution.name }}</div>
      </div>
      <div>
        <button
          class="add-account-button"
          type="button"
          :disabled="loading"
          @click="handleAddAccountClick"
        >
          <CircleSpinner v-if="loading" />
          <span v-else>Add Account</span>
        </button>
      </div>
    </div>
    <div class="accounts-list">
      <div
        class="account-row"
        v-for="account in institution.accounts"
        :key="account.id"
      >
        <div class="account-card">{{ account.name }}</div>
      </div>
    </div>
    <div
      class="add-account-card"
      v-if="showAddForm"
    >
      <AddAccountForm
        :availableAccounts="availableAccounts"
        :isSubmitting="addingAccount"
        @submit="handleFormSubmit"
      />
    </div>
  </div>
</template>

<style scoped>
  .institution-container {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .institution-card,
  .account-card,
  .add-account-card {
    display: flex;
    align-items: center;
    padding: 1rem;
    border-radius: 0.25rem;
    background: var(--bg-surface);
  }

  .institution-card,
  .account-card {
    position: relative;
  }

  .institution-card > div {
    flex: 1;
  }

  .institution-card > div:last-of-type {
    display: flex;
    justify-content: flex-end;
  }

  .institution-card::before {
    content: '';
    position: absolute;
    top: 50%;
    left: -0.25rem;
    transform: translateY(-50%);
    width: 0.5rem;
    height: 0.5rem;
    border-radius: 50%;
    background: var(--brand-primary);
    z-index: 1;
  }

  .institution-card::after {
    content: '';
    position: absolute;
    top: 50%;
    left: -1px;
    height: calc(50% + 0.5rem);
    width: 2px;
    background: var(--brand-primary);
  }

  .add-account-card {
    margin-left: 2rem;
    gap: 0.5rem;
  }

  .accounts-list {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .account-row {
    position: relative;
    padding-left: 2rem;
  }

  .account-row::before {
    content: '';
    position: absolute;
    top: 0;
    left: -1px;
    height: calc(100% + 0.5rem);
    width: 2px;
    background: var(--brand-primary);
  }

  .account-row:not(:first-child)::before {
    top: -0.5rem;
    height: calc(100% + 1rem);
  }

  .account-row:last-child::before {
    height: 50%;
  }

  .account-row:not(:first-child):last-child::before {
    top: -0.5rem;
    height: calc(50% + 0.5rem);
  }

  .account-row::after {
    content: '';
    position: absolute;
    top: 50%;
    left: -1px;
    width: calc(2rem + 1px);
    height: 2px;
    background: var(--brand-primary);
    transform: translateY(-50%);
  }

  .account-card::before {
    content: '';
    position: absolute;
    top: 50%;
    left: -0.25rem;
    transform: translateY(-50%);
    width: 0.5rem;
    height: 0.5rem;
    border-radius: 50%;
    background: var(--brand-primary);
    z-index: 1;
  }
</style>
