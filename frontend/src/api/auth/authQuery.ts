import { API_URL } from '@/constants/api'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'

export const useAuthQuery = () =>
  useQuery({
    queryKey: ['auth'],
    queryFn: async () => {
      const response = await axios.get(`${API_URL}/auth`, {
        withCredentials: true
      })

      return response.data
    },
    retry: 0
  })
