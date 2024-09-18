import { API_URL } from '@/constants/api'
import type { BrokerDto } from '@/dtos/broker/brokerDto'
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
