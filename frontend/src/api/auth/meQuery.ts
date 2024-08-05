import type { User } from '@/composables/identityComposable'
import { API_URL } from '@/constants/Api'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'

export const useMeQuery = () =>
  useQuery({
    queryKey: ['me'],
    queryFn: async () => {
      const response = await axios.get<User>(`${API_URL}/auth/me`, {
        withCredentials: true
      })

      return response.data
    },
    retry: () => false
  })
