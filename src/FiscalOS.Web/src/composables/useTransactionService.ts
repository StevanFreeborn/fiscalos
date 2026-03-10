import { ClientConfig, ClientFactoryKey } from '@/services/client';
import { TransactionServiceFactoryKey } from '@/services/transactionService';
import type { UserStore } from '@/stores/userStore';
import { inject } from 'vue';

export function useTransactionService(store: UserStore) {
  const clientFactory = inject(ClientFactoryKey);
  const transactionServiceFactory = inject(TransactionServiceFactoryKey);

  if (clientFactory === undefined) {
    throw new Error('Failed to inject client factory.');
  }

  if (transactionServiceFactory === undefined) {
    throw new Error('Failed to inject transaction service factory.');
  }

  const clientConfig = new ClientConfig(
    { Authorization: `Bearer ${store.user?.token}` },
    true,
    store.refreshAccessToken
  );
  const client = clientFactory.create(clientConfig);
  const transactionService = transactionServiceFactory.create(client);

  return transactionService;
}
