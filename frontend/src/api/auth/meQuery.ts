import type { User } from '@/composables/identityComposable'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'

export const useMeQuery = () =>
  useQuery({
    queryKey: ['me'],
    queryFn: async () => {
      const response = await axios.get<User>('http://localhost:5182/auth/me', {
        withCredentials: true
      })

      return response.data
    },
    retry: () => false
  })
