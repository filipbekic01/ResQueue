import { useMeQuery } from '@/api/auth/meQuery'

export function useIdentity() {
  const query = useMeQuery()

  return {
    query
  }
}
