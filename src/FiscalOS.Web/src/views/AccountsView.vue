<script setup lang="ts">
  import CircleSpinner from '@/components/CircleSpinner.vue';
  import { useInstitutionService } from '@/composables/useInstitutionService';
  import type { AvailableAccount, Institution } from '@/services/institutionService';
  import { useUserStore } from '@/stores/userStore';
  import {
    usePlaidLink,
    type PlaidLinkOnSuccessMetadata,
    type PlaidLinkOptions,
  } from '@jcss/vue-plaid-link';
  import { nextTick, ref, watch } from 'vue';

  type InstitutionData =
    | {
        status: 'loading';
      }
    | { status: 'loaded'; data: Institution[] }
    | { status: 'errored'; errors: Error[] };

  const institutionsData = ref<InstitutionData>({ status: 'loading' });
  const targetInstitutionOfAdd = ref<Institution | null>(null);
  const availableAccounts = ref<AvailableAccount[]>([]);
  const targetAccountOfAdd = ref<AvailableAccount | null>(null);
  const loadingInstitutionId = ref<string | null>(null);
  const accountSelectRefs = ref<HTMLSelectElement[]>([]);
  const addingAccount = ref(false);
  const plaidOptions = ref<PlaidLinkOptions>({
    token: '',
    onSuccess: handleSuccess,
  });

  const userStore = useUserStore();
  const institutionService = useInstitutionService(userStore);
  const { open } = usePlaidLink(plaidOptions);

  async function handleSuccess(publicToken: string, metadata: PlaidLinkOnSuccessMetadata) {
    if (metadata.institution == null) {
      alert('Institution information is missing from Plaid response. Please try again.');
      return;
    }

    const connectResult = await institutionService.connect(
      publicToken,
      metadata.institution.institution_id
    );

    if (connectResult.err) {
      alert(connectResult.val.map(e => e.message).join('\n'));
      return;
    }

    institutionsData.value = { status: 'loading' };
  }

  async function institutionsDataWatcher(data: InstitutionData) {
    if (data.status !== 'loading') {
      return;
    }

    const institutionsResult = await institutionService.getInstitutions();

    if (institutionsResult.err) {
      institutionsData.value = {
        status: 'errored',
        errors: institutionsResult.val,
      };

      return;
    }

    institutionsData.value = {
      status: 'loaded',
      data: institutionsResult.val,
    };
  }

  watch(institutionsData, institutionsDataWatcher, { immediate: true });

  async function handleAddInstitutionClick() {
    const linkTokenResult = await institutionService.createLinkToken();

    if (linkTokenResult.err) {
      alert(linkTokenResult.val.map(e => e.message).join('\n'));
      return;
    }

    plaidOptions.value = {
      ...plaidOptions.value,
      token: linkTokenResult.val.linkToken,
    };

    await nextTick();

    open();
  }

  async function handleAddAccountClick(institution: Institution) {
    loadingInstitutionId.value = institution.id;
    const accountsResult = await institutionService.getAvailableAccounts(institution.id);
    loadingInstitutionId.value = null;

    if (accountsResult.err) {
      alert('Failed to retrieve accounts to add');
      return;
    }

    const notAlreadyAddedAccounts = accountsResult.val.filter(a => {
      return institution.accounts.some(ac => ac.providerId === a.providerId) === false;
    });

    availableAccounts.value = notAlreadyAddedAccounts;
    targetInstitutionOfAdd.value = institution;

    await nextTick();

    if (institutionsData.value.status === 'loaded') {
      const index = institutionsData.value.data.findIndex(i => i.id === institution.id);
      accountSelectRefs.value[index]?.focus();
    }
  }

  async function handleAccountAddClick() {
    if (targetAccountOfAdd.value === null) {
      return;
    }

    addingAccount.value = true;

    const request = {
      plaidInstitutionId: targetAccountOfAdd.value.providerInstitutionId,
      plaidAccountId: targetAccountOfAdd.value.providerId,
      plaidAccountName: targetAccountOfAdd.value.providerName,
    };

    try {
      const res = await fetch('/api/accounts', {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${userStore.user?.token}`,
        },
        body: JSON.stringify(request),
      });

      if (res.ok === false) {
        alert('Add failed');
        addingAccount.value = false;
        return;
      }

      targetInstitutionOfAdd.value = null;
      addingAccount.value = false;
      institutionsData.value = { status: 'loading' };
    } catch (error) {
      addingAccount.value = false;
      console.error(error);
    }
  }
</script>

<template>
  <div class="header">
    <h1>Accounts</h1>
    <button
      class="add-institution-button"
      type="button"
      @click="handleAddInstitutionClick"
    >
      Add Institution
    </button>
  </div>
  <div
    v-if="institutionsData.status === 'loaded'"
    class="institutions-container"
  >
    <div
      class="institution-container"
      v-for="institution in institutionsData.data"
      :key="institution.id"
    >
      <div class="institution-card">
        <div>
          <div>{{ institution.name }}</div>
        </div>
        <div>
          <button
            class="add-account-button"
            type="button"
            :disabled="loadingInstitutionId === institution.id"
            @click="handleAddAccountClick(institution)"
          >
            <CircleSpinner v-if="loadingInstitutionId === institution.id" />
            <span v-else>Add Account</span>
          </button>
        </div>
      </div>
      <div class="accounts-list">
        <div
          class="account-row"
          v-for="accounts in institution.accounts"
          :key="accounts.id"
        >
          <div class="account-card">{{ accounts.name }}</div>
        </div>
      </div>
      <div
        class="add-account-card"
        v-if="targetInstitutionOfAdd != null && targetInstitutionOfAdd.id === institution.id"
      >
        <div class="select-account-container">
          <label for="account-select">Select account to add:</label>
          <select
            id="account-select"
            ref="accountSelectRefs"
            v-model="targetAccountOfAdd"
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
        </div>
        <button
          class="add-account-button"
          type="button"
          :disabled="addingAccount"
          @click="handleAccountAddClick"
        >
          <CircleSpinner v-if="addingAccount" />
          <span v-else>Add</span>
        </button>
      </div>
    </div>
  </div>
  <div v-if="institutionsData.status === 'errored'">Failed to load institutions</div>
</template>

<style scoped>
  .header {
    display: flex;
    gap: 1rem;
    align-items: center;
  }

  .institutions-container {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    padding: 1rem;
  }

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
  }

  .add-account-card {
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

  .select-account-container {
    display: flex;
    gap: 0.25rem;
    align-items: center;
  }

  .select-account-container > select {
    padding: 0.25rem;
    border-radius: 0.25rem;
  }

  .add-institution-button,
  .add-account-button {
    background: var(--bg-element);
    padding: 0.25rem 0.5rem;
    border-radius: 0.25rem;
  }

  .add-account-button:disabled {
    cursor: not-allowed;
    opacity: 0.6;
  }
</style>
