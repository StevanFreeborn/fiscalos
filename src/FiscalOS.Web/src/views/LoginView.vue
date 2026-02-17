<script setup lang="ts">
  import { useUserStore } from '@/stores/userStore';
  import { useRouter } from 'vue-router';

  const { logUserIn } = useUserStore();
  const router = useRouter();

  async function handleSubmit(e: SubmitEvent) {
    const data = Object.fromEntries(new FormData(e.currentTarget as HTMLFormElement));
    const response = await fetch('/api/auth/login', {
      headers: {
        'Content-Type': 'application/json',
      },
      method: 'POST',
      body: JSON.stringify(data),
    });

    if (response.ok === false) {
      alert('Failed to login');
      return;
    }

    const responseBody = await response.json();

    logUserIn(responseBody.accessToken)
    router.push('/');
  }
</script>

<template>
  <h1>Login View</h1>
  <form v-on:submit.prevent="handleSubmit">
    <div>
      <label for="username">Username</label>
      <input type="text" name="username" id="username">
    </div>
    <div>
      <label for="password">Password</label>
      <input type="password" name="password" id="password">
    </div>
    <div>
      <button type="submit">Login</button>
    </div>
  </form>
</template>

<style scoped></style>
