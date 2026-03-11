<script setup lang="ts">
  import InstitutionCard from '@/components/InstitutionCard.vue';
  import { useInstitutionService } from '@/composables/useInstitutionService';
  import type { Institution } from '@/services/institutionService';
  import { useUserStore } from '@/stores/userStore';
  import {
    usePlaidLink,
    type PlaidLinkOnSuccessMetadata,
    type PlaidLinkOptions,
  } from '@jcss/vue-plaid-link';
  import { nextTick, ref, watch } from 'vue';

  type InstitutionData =
    | { status: 'loading' }
    | { status: 'loaded'; data: Institution[] }
    | { status: 'errored'; errors: Error[] };

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

  const plaidOptions = ref<PlaidLinkOptions>({
    token: '',
    onSuccess: handleSuccess,
  });

  const { open } = usePlaidLink(plaidOptions);

  const institutionsData = ref<InstitutionData>({ status: 'loading' });

  const userStore = useUserStore();
  const institutionService = useInstitutionService(userStore);

  async function institutionsDataWatcher(data: InstitutionData) {
    if (data.status !== 'loading') {
      return;
    }

    const institutionsResult = await institutionService.getInstitutions();

    if (institutionsResult.err) {
      institutionsData.value = { status: 'errored', errors: institutionsResult.val };
      return;
    }

    institutionsData.value = { status: 'loaded', data: institutionsResult.val };
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
    <InstitutionCard
      v-for="institution in institutionsData.data"
      :key="institution.id"
      :institution="institution"
      @accountAdded="institutionsData = { status: 'loading' }"
    />
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

  .add-institution-button {
    background: var(--bg-element);
    padding: 0.25rem 0.5rem;
    border-radius: 0.25rem;
  }
</style>
