<script setup lang="ts">
import { useAuthService } from '@/composables/useAuthService';
import { useUserStore } from '@/stores/userStore';
import { useRouter } from 'vue-router';

const router = useRouter();
const userStore = useUserStore();
const authService = useAuthService(userStore);

async function handleSubmit(e: SubmitEvent) {
  const data = Object.fromEntries(new FormData(e.currentTarget as HTMLFormElement));
  const loginResult = await authService.login(
    data['username']!.toString(),
    data['password']!.toString()
  );

  if (loginResult.err) {
    alert(loginResult.val.map(e => e.message).join('\n'));
    return;
  }

  userStore.logUserIn(loginResult.val.accessToken);
  router.push('/');
}
</script>

<template>
  <form @submit.prevent="handleSubmit">
    <div>
      <label for="username">Username</label>
      <input type="text" name="username" id="username" />
    </div>
    <div>
      <label for="password">Password</label>
      <input type="password" name="password" id="password" />
    </div>
    <div>
      <button type="submit">Login</button>
    </div>
  </form>
</template>

<style scoped>
  form,
  form > div {
    display: flex;
    flex-direction: column;
    background-color: var(--bg-surface);
  }

  form {
    gap: 1rem;
    padding: 1rem;
    border-radius: 0.25rem;
  }

  form > div {
    gap: 0.25rem;
  }

  form > div label {
    font-weight: 700;
  }

  form > div input {
    padding: 0.5rem;
    border-radius: 0.25rem;
    border: 1px solid black;
    background-color: var(--bg-element);
  }

  form > div button {
    background-color: var(--brand-primary);
    padding: 0.5rem 0.25rem;
    border-radius: 0.25rem;
  }
</style>
