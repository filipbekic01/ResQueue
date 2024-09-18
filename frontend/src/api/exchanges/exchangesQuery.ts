import { API_URL } from '@/constants/api'
import type { ExchangeDto } from '@/dtos/exchanges/exchangeDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useExchangesQuery = (brokerId: MaybeRef<string | undefined>) =>
  useQuery({
    queryKey: ['exchanges', brokerId],
    queryFn: async () => {
      const response = await axios.get<ExchangeDto[]>(`${API_URL}/exchanges/${toValue(brokerId)}`, {
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(brokerId))
  })
