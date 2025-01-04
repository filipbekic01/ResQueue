import { API_URL } from '@/constants/api'
import type { QueueViewDto } from '@/dtos/queue/queueViewDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useQueuesViewQuery = (
  refetchInterval: MaybeRef<number> = 5000,
  enabled: MaybeRef<boolean> = true,
) =>
  useQuery({
    queryKey: ['queues-view'],
    queryFn: async () => {
      const response = await axios.get<QueueViewDto[]>(`${API_URL}/queues/view`, {
        withCredentials: true,
      })

      return response.data
    },
    enabled: computed(() => !!toValue(enabled)),
    refetchInterval: refetchInterval,
  })
