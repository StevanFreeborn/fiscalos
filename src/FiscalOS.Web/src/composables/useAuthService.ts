import {  AuthServiceFactoryKey } from "@/services/authService";
import { ClientConfig, ClientFactoryKey } from "@/services/client";
import type { UserStore } from "@/stores/userStore";
import { inject } from "vue";

export function useAuthService(store: UserStore) {
  const clientFactory = inject(ClientFactoryKey);
  const authServiceFactory = inject(AuthServiceFactoryKey);

  if (clientFactory === undefined) {
    throw new Error("Failed to inject client factory.")
  }

  if (authServiceFactory === undefined) {
    throw new Error("Failed to inject auth service factory.")
  }

  const clientConfig = new ClientConfig(
    { Authorization: `Bearer ${store.user?.token}`},
    true,
    store.refreshAccessToken
  );
  const client = clientFactory.create(clientConfig);
  const authService = authServiceFactory.create(client);

  return authService;
}
