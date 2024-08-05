import { API_URL } from '@/constants/Api'
import type { BrokerDto } from '@/dtos/brokerDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'

export const useBrokersQuery = () =>
  useQuery({
    queryKey: ['brokers'],
    queryFn: async () => {
      const response = await axios.get<BrokerDto[]>(`${API_URL}/brokers`, {
        withCredentials: true
      })

      return response.data
    }
  })
