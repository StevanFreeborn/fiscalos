import { AccountServiceFactoryKey } from '@/services/accountService';
import type { UserStore } from '@/stores/userStore';
import { useService } from './useService';

export function useAccountService(store: UserStore) {
  return useService(store, AccountServiceFactoryKey);
}
