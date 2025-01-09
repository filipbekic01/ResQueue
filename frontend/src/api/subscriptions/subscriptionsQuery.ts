import { API_URL } from '@/constants/api'
import type { SubscriptionDto } from '@/dtos/subscriptions/subscriptionDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { type MaybeRef } from 'vue'

export const useSubscriptionsQuery = (refetchInterval: MaybeRef<number> = 5000) =>
  useQuery({
    queryKey: ['subscriptions'],
    queryFn: async () => {
      const response = await axios.get<SubscriptionDto[]>(`${API_URL}/subscriptions`, {
        withCredentials: true,
      })

      return response.data
    },
    refetchInterval: refetchInterval,
  })
