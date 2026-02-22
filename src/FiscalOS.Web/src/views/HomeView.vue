<script setup lang="ts">
  import { useInstitutionService } from '@/composables/useInstitutionService';
  import type { Institution } from '@/services/institutionService';
  import { useUserStore } from '@/stores/userStore';
  import { usePlaidLink, type PlaidLinkOptions } from '@jcss/vue-plaid-link';
  import { nextTick, ref, watch } from 'vue';

  const userStore = useUserStore();
  const institutionService = useInstitutionService(userStore);

  type InstitutionData =
    | {
        status: 'loading';
      }
    | { status: 'loaded'; data: Institution[] }
    | { status: 'errored'; errors: Error[] };

  const institutionsData = ref<InstitutionData>({ status: 'loading' });

  const plaidOptions = ref<PlaidLinkOptions>({
    token: '',
    onSuccess: async (publicToken, metadata) => {
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
    },
    onLoad: () => console.log('loaded'),
    onExit: () => console.log('exit'),
  });
  const { open } = usePlaidLink(plaidOptions);

  watch(
    institutionsData,
    async data => {
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
    },
    { immediate: true }
  );

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
  <div>
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
      class="institution-card"
      v-for="institution in institutionsData.data"
      v-bind:key="institution.id"
    >
      <div>
        <div>{{ institution.name }}</div>
      </div>
      <div>
        <button
          class="add-account-button"
          type="button"
        >
          Add Account
        </button>
      </div>
    </div>
  </div>
  <div v-if="institutionsData.status === 'errored'">Failed to load institutions</div>
</template>

<style scoped>
  .institutions-container {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    padding: 1rem;
  }

  .institution-card {
    display: flex;
    align-items: center;
    padding: 1rem;
    border-radius: 0.25rem;
    background: var(--bg-surface);
  }

  .institution-card > div {
    flex: 1;
  }

  .institution-card > div:last-of-type {
    display: flex;
    justify-content: flex-end;
  }

  .add-institution-button,
  .add-account-button {
    background: var(--bg-element);
    padding: 0.25rem 0.5rem;
    border-radius: 0.25rem;
  }
</style>
