import { AuthServiceFactoryKey } from '@/services/authService';
import type { UserStore } from '@/stores/userStore';
import { useService } from './useService';

export function useAuthService(store: UserStore) {
  return useService(store, AuthServiceFactoryKey);
}
