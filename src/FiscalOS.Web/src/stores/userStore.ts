import { AuthServiceFactoryKey } from '@/services/authService';
import { ClientConfig, ClientFactoryKey } from '@/services/client';
import { jwtDecode } from 'jwt-decode';
import { defineStore } from 'pinia';
import { inject, ref } from 'vue';

export const USER_KEY = 'fiscalos_auth';

export type User = {
  id: string;
  expiresAtInSeconds: number;
  token: string;
  sidebarCollapsed: boolean;
};

type JwtTokenPayload = {
  sub: string;
  exp: number;
};

function getUserFromLocalStorage(): User | null {
  const user = localStorage.getItem(USER_KEY);
  return user === null ? null : JSON.parse(user);
}

function saveUserToLocalSotrage(user: User) {
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export type UserStore = ReturnType<typeof useUserStore>;

export const useUserStore = defineStore('userStore', () => {
  const user = ref<User | null>(getUserFromLocalStorage());
  const clientFactory = inject(ClientFactoryKey);
  const authServiceFactory = inject(AuthServiceFactoryKey);

  function logUserIn(jwtToken: string) {
    const { sub, exp } = jwtDecode<JwtTokenPayload>(jwtToken);
    const loggedInUser: User = {
      id: sub,
      expiresAtInSeconds: exp,
      token: jwtToken,
      sidebarCollapsed: false,
    };
    user.value = loggedInUser;
    saveUserToLocalSotrage(loggedInUser);
  }

  function logUserOut() {
    localStorage.removeItem(USER_KEY);
    user.value = null;
  }

  async function refreshAccessToken(originalRequest: Request) {
    const unauthorizedResponse = new Response(null, { status: 401 });

    if (user.value === null || !clientFactory || !authServiceFactory) {
      logUserOut();
      return { response: unauthorizedResponse, accessToken: '' };
    }

    const client = clientFactory.create(
      new ClientConfig({ Authorization: `Bearer ${user.value.token}` }, true)
    );

    const authService = authServiceFactory.create(client);

    const refreshResult = await authService.refreshToken();

    if (refreshResult.err) {
      logUserOut();
      return { response: unauthorizedResponse, accessToken: '' };
    }

    logUserIn(refreshResult.val.accessToken);

    originalRequest.headers.set('Authorization', `Bearer ${refreshResult.val.accessToken}`);
    const response = await fetch(originalRequest);

    return { response, accessToken: refreshResult.val.accessToken };
  }

  async function toggleSidebar() {
    if (user.value === null) {
      return;
    }

    const updatedUser = {
      ...user.value,
      sidebarCollapsed: !user.value?.sidebarCollapsed,
    }
    user.value = updatedUser;
    saveUserToLocalSotrage(updatedUser);
  }

  return {
    user: user,
    logUserIn,
    logUserOut,
    refreshAccessToken,
    toggleSidebar,
  };
});
