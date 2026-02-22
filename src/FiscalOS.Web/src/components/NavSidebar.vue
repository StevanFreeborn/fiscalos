<script setup lang="ts">
  import LeftArrowIcon from '@/components/icons/RightArrowIcon.vue';
  import { useUserStore } from '@/stores/userStore';
  import { computed } from 'vue';
import { useRouter } from 'vue-router';

  const userStore = useUserStore();
  const router = useRouter();

  const asideClasses = computed(() => ({
    collapsed: userStore.user?.sidebarCollapsed,
  }));

  function handleToggleButtonClick() {
    userStore.toggleSidebar();
  }

  function handleLogout() {
    // TODO: Also need to log user
    // out on the server...which is
    // basically just clearing
    // refresh token cookie and revoking
    // it in the database
    userStore.logUserOut();
    router.push({ path: '/public/login' });
  }
</script>

<template>
  <aside :class="asideClasses">
    <button
      @click="handleToggleButtonClick"
      type="button"
      class="toggle-button"
    >
      <LeftArrowIcon />
    </button>
    <button
      class="logout-button"
      type="button"
      @click="handleLogout"
    >
      Logout
    </button>
  </aside>
</template>

<style scoped>
  aside {
    --sidebar-width: 15.625rem;
    --button-size: 1.5rem;
    --transition-duration: 0.5s;
    --transition-function: ease-in-out;
    position: relative;
    width: var(--sidebar-width);
    height: 100%;
    z-index: 999;
    background: var(--bg-surface);
    transition-property: width, transform;
    transition-duration: var(--transition-duration);
    transition-timing-function: var(--transition-function);
  }

  @media screen and (max-width: 48rem) {
    aside {
      position: absolute;
      right: 0;
    }
  }

  .logout-button {
    background: var(--bg-element);
    padding: 0.25rem 0.5rem;
    border-radius: 0.25rem;
  }

  .toggle-button {
    display: flex;
    align-items: center;
    justify-content: center;
    position: absolute;
    top: 3rem;
    left: calc(-1 * var(--button-size) / 2);
    height: var(--button-size);
    width: var(--button-size);
    background: var(--bg-element);
    border-radius: 50%;
    transition-property: transform;
    transition-duration: var(--transition-duration);
    transition-timing-function: var(--transition-function);
  }

  aside.collapsed {
    width: calc(0.125rem + var(--button-size) / 2);
  }

  aside.collapsed .toggle-button {
    transform: rotate(180deg);
  }

  .toggle-button svg {
    --size: 1rem;
    width: var(--size);
    height: var(--size);
  }
</style>
