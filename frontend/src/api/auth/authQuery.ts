import { API_URL } from '@/constants/api'
import type { AuthDto } from '@/dtos/authDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'

export const useAuthQuery = () =>
  useQuery({
    queryKey: ['auth'],
    queryFn: async () => {
      const response = await axios.get<AuthDto>(`${API_URL}/auth`, {
        withCredentials: true
      })

      return response.data
    },
    retry: 0
  })
