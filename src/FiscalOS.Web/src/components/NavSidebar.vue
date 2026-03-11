<script setup lang="ts">
  import LeftArrowIcon from '@/components/icons/RightArrowIcon.vue';
  import { useAuthService } from '@/composables/useAuthService';
  import { useUserStore } from '@/stores/userStore';
  import { computed } from 'vue';
  import { RouterLink, useRouter } from 'vue-router';
  import RightArrowBracketIcon from './icons/RightArrowBracketIcon.vue';

  const userStore = useUserStore();
  const router = useRouter();
  const authService = useAuthService(userStore);

  const asideClasses = computed(() => ({
    collapsed: userStore.user?.sidebarCollapsed,
  }));

  function handleToggleButtonClick() {
    userStore.toggleSidebar();
  }

  async function handleLogout() {
    const logoutResult = await authService.logout();

    if (logoutResult.err) {
      alert(logoutResult.val.map(e => e.message).join('\n'));
      return;
    }

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
    <div class="sidebar-content">
      <div class="top">
        <nav>
          <ul>
            <li>
              <RouterLink to="/accounts">Accounts</RouterLink>
            </li>
            <li>
              <RouterLink to="/transactions">Transactions</RouterLink>
            </li>
          </ul>
        </nav>
      </div>
      <div class="bottom">
        <button
          class="logout-button"
          type="button"
          @click="handleLogout"
        >
          <RightArrowBracketIcon />
          Logout
        </button>
      </div>
    </div>
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

  .sidebar-content {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    height: 100%;
    gap: 3rem;
    flex: 1;
    overflow: hidden;
    opacity: 1;
    visibility: visible;
    transition-property: opacity, visibility;
    transition-duration: var(--transition-duration);
    transition-timing-function: var(--transition-function);
  }

  .top {
    padding: 1rem;
  }

  nav ul {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  nav a {
    display: flex;
    width: 100%;
    padding: 0.5rem 1rem;
    border-radius: 0.25rem;
  }

  nav a.router-link-exact-active,
  nav a:hover {
    background: var(--bg-element);
  }

  nav a.router-link-exact-active {
    position: relative;
  }

  nav a.router-link-exact-active::before {
    content: ' ';

    position: absolute;
    left: 0;
    height: 1.25rem;
    background: red;
    border: 2px solid var(--brand-primary);
  }

  .bottom {
    padding: 1rem;
  }

  .logout-button {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    width: 100%;
    background: var(--bg-element);
    padding: 0.5rem 1rem;
    border-radius: 0.25rem;
  }

  .logout-button svg {
    --size: 1.5rem;
    width: var(--size);
    height: var(--size);
    fill: var(--text-primary);
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

  .toggle-button svg {
    --size: 1rem;
    width: var(--size);
    height: var(--size);
    fill: var(--text-primary);
  }

  aside.collapsed {
    width: calc(0.125rem + var(--button-size) / 2);
  }

  aside.collapsed .toggle-button {
    transform: rotate(180deg);
  }

  aside.collapsed .sidebar-content {
    opacity: 0;
    visibility: hidden;
    pointer-events: none;
  }
</style>
