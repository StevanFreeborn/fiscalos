import { TransactionServiceFactoryKey } from '@/services/transactionService';
import type { UserStore } from '@/stores/userStore';
import { useService } from './useService';

export function useTransactionService(store: UserStore) {
  return useService(store, TransactionServiceFactoryKey);
}
