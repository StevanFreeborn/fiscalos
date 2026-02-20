<script setup lang="ts">
import { useInstitutionService } from '@/composables/useInstitutionService';
import { useUserStore } from '@/stores/userStore';
import { usePlaidLink, type PlaidLinkOptions } from '@jcss/vue-plaid-link';
import { nextTick, ref } from 'vue';
import { useRouter } from 'vue-router';

const userStore = useUserStore();
const router = useRouter();
const institutionService = useInstitutionService(userStore);
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
      alert(connectResult.val.map(e => e.message).join('\n'))
      return;
    }
  },
  onLoad: () => console.log('loaded'),
  onExit: () => console.log('exit'),
});
const { open } = usePlaidLink(plaidOptions);

function handleLogout() {
  // TODO: Also need to log user
  // out on the server...which is
  // basically just clearing
  // refresh token cookie and revoking
  // it in the database
  userStore.logUserOut();
  router.push({ path: '/public/login' });
}

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
  <h1>Home View</h1>
  <button class="logout-button" type="button" @click="handleLogout">
    Logout
  </button>
  <div>
    <button class="add-institution-button" type="button" @click="handleAddInstitutionClick">
      Add Institution
    </button>
  </div>
</template>

<style scoped>
.logout-button,
.add-institution-button {
  background: var(--bg-element);
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
}
</style>
