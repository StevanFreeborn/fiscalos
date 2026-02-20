<script setup lang="ts">
  import LeftArrowIcon from '@/components/icons/RightArrowIcon.vue';
  import { computed, ref } from 'vue';

  const isCollapsed = ref(false);
  const asideClasses = computed(() => ({
    collapsed: isCollapsed.value,
  }));

  function handleToggleButtonClick() {
    isCollapsed.value = !isCollapsed.value;
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
