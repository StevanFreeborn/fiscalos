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

    alert(loginResult.val.join('\n'));
    return;
  }

  userStore.logUserIn(loginResult.val.accessToken);
  router.push('/');
}
</script>

<template>
  <h1>Login View</h1>
  <form v-on:submit.prevent="handleSubmit">
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

<style scoped></style>
