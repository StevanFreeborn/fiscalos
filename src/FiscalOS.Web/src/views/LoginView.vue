<script setup lang="ts">
import LoginForm, { type LoginFormState } from '@/components/LoginForm.vue';
import { useAuthService } from '@/composables/useAuthService';
import { useUserStore } from '@/stores/userStore';
import { useRouter } from 'vue-router';

const router = useRouter();
const userStore = useUserStore();
const authService = useAuthService(userStore);

async function handleValideSubmit(formState: LoginFormState) {
  const loginResult = await authService.login(formState.username.value, formState.password.value);

  if (loginResult.err) {
    alert(loginResult.val.map(e => e.message).join('\n'));
    return;
  }

  userStore.logUserIn(loginResult.val.accessToken);
  router.push('/');
}
</script>

<template>
  <LoginForm :onValidSubmit="handleValideSubmit" />
</template>

<style scoped></style>
