<script setup lang="ts">
  import { RouterView } from 'vue-router';
  import NavSidebar from './NavSidebar.vue';
</script>

<template>
  <div class="layout">
    <main>
      <RouterView />
    </main>
    <NavSidebar />
  </div>
</template>

<style scoped>
  .layout {
    display: flex;
    height: 100%;
    position: relative;
  }

  main {
    flex: 1;
    padding: 1rem;
  }
</style>
