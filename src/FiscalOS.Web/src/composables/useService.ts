import type { InjectionKey } from 'vue';
import { inject } from 'vue';
import { ClientConfig, ClientFactoryKey, type IClient } from '@/services/client';
import type { UserStore } from '@/stores/userStore';

export interface IServiceFactory<TService> {
  create: (client: IClient) => TService;
}

export function useService<TService>(
  store: UserStore,
  serviceFactoryKey: InjectionKey<IServiceFactory<TService>>
): TService {
  const clientFactory = inject(ClientFactoryKey);
  const serviceFactory = inject(serviceFactoryKey);

  if (clientFactory === undefined) {
    throw new Error('Failed to inject client factory.');
  }

  if (serviceFactory === undefined) {
    throw new Error('Failed to inject service factory.');
  }

  const clientConfig = new ClientConfig(
    { Authorization: `Bearer ${store.user?.token}` },
    true,
    store.refreshAccessToken
  );

  const client = clientFactory.create(clientConfig);
  return serviceFactory.create(client);
}
