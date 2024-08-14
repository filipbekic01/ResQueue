import { API_URL } from '@/constants/api'
import type { UserDto } from '@/dtos/userDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'

export const useMeQuery = () =>
  useQuery({
    queryKey: ['me'],
    queryFn: async () => {
      const response = await axios.get<UserDto>(`${API_URL}/auth/me`, {
        withCredentials: true
      })

      return response.data
    },
    retry: () => false
  })
