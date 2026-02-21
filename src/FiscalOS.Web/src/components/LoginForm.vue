<script setup lang="ts">
import { ref } from 'vue';

export type LoginFormState = {
  isLoggingIn: boolean;
  username: {
    value: string;
    error: string;
  };
  password: {
    value: string;
    error: string;
  };
};

const props = defineProps<{
  onValidSubmit?: (formState: LoginFormState) => Promise<void> | void;
}>();

const formState = ref<LoginFormState>({
  isLoggingIn: false,
  username: {
    value: '',
    error: '',
  },
  password: {
    value: '',
    error: '',
  },
});

function handleUsernameInput(e: Event) {
  formState.value.username.value = (e.currentTarget as HTMLInputElement).value;

  if (formState.value.username.error.trim()) {
    formState.value.username.error = '';
  }
}

function handlePasswordInput(e: Event) {
  formState.value.password.value = (e.currentTarget as HTMLInputElement).value;

  if (formState.value.password.error.trim()) {
    formState.value.password.error = '';
  }
}

function validateFormState() {
  let isValid = true;

  if (!formState.value.username.value.trim()) {
    formState.value.username.error = 'Username is required';
    isValid = false;
  }

  if (!formState.value.password.value.trim()) {
    formState.value.password.error = 'Password is required';
    isValid = false;
  }

  return isValid;
}

async function handleSubmit() {
  formState.value.isLoggingIn = true;

  try {
    if (validateFormState() === false) {
      return;
    }

    if (props.onValidSubmit !== undefined) {
      await props.onValidSubmit(formState.value);
    }

  } finally {
    formState.value.isLoggingIn = false;
  }
}
</script>

<template>
  <form @submit.prevent="handleSubmit">
    <div>
      <label for="username">Username</label>
      <input type="text" name="username" id="username" :value="formState.username.value" @input="handleUsernameInput" />
      <div class="error">{{ formState.username.error }}</div>
    </div>
    <div>
      <label for="password">Password</label>
      <input type="password" name="password" id="password" :value="formState.password.value"
        @input="handlePasswordInput" />
      <div class="error">{{ formState.password.error }}</div>
    </div>
    <div>
      <button type="submit" :disabled="formState.isLoggingIn">
        Login
      </button>
    </div>
  </form>
</template>

<style scoped>
form,
form>div {
  display: flex;
  flex-direction: column;
  background-color: var(--bg-surface);
}

form {
  gap: 1rem;
  padding: 1rem;
  border-radius: 0.25rem;
}

form>div {
  gap: 0.25rem;
}

form>div label {
  font-weight: 700;
}

form>div input {
  padding: 0.5rem;
  border-radius: 0.25rem;
  border: 1px solid black;
  background-color: var(--bg-element);
}

form>div button {
  background-color: var(--brand-primary);
  padding: 0.5rem 0.25rem;
  border-radius: 0.25rem;
}

form>div button:disabled {
  opacity: 50%;
  pointer-events: none;
}

form>div .error {
  color: var(--state-error);
  font-size: 0.85rem;
}
</style>
