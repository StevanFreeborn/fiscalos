import { ClientConfig, ClientFactoryKey } from "@/services/client";
import { InstituionServiceFactoryKey } from "@/services/institutionService";
import type { UserStore } from "@/stores/userStore";
import { inject } from "vue";

export function useInstitutionService(store: UserStore) {
  const clientFactory = inject(ClientFactoryKey);
  const institutionServiceFactory = inject(InstituionServiceFactoryKey);

  if (clientFactory === undefined) {
    throw new Error("Failed to inject client factory.")
  }

  if (institutionServiceFactory === undefined) {
    throw new Error("Failed to inject institution service factory.")
  }

  const clientConfig = new ClientConfig(
    { Authorization: `Bearer ${store.user?.token}`},
    true,
    store.refreshAccessToken
  );
  const client = clientFactory.create(clientConfig);
  const institutionService = institutionServiceFactory.create(client);

  return institutionService;
}
