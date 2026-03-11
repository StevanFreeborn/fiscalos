import { InstitutionServiceFactoryKey } from '@/services/institutionService';
import type { UserStore } from '@/stores/userStore';
import { useService } from './useService';

export function useInstitutionService(store: UserStore) {
  return useService(store, InstitutionServiceFactoryKey);
}
