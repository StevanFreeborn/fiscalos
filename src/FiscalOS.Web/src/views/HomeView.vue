<script setup lang="ts">
import { useUserStore } from '@/stores/userStore';
import { useRouter } from 'vue-router';

const { logUserOut } = useUserStore();
const router = useRouter();

function handleLogout() {
  // TODO: Also need to log user
  // out on the server...which is
  // basically just clearing
  // refresh token cookie and revoking
  // it in the database
  logUserOut();
  router.push({ path: '/public/login' });
}
</script>

<template>
  <h1>Home View</h1>
  <button type="button" v-on:click="handleLogout">
    Logout
  </button>
</template>

<style scoped></style>
