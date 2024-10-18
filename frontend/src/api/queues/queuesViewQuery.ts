import { API_URL } from '@/constants/api'
import type { QueueViewDto } from '@/dtos/queue/queueViewDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useQueuesViewQuery = (brokerId: MaybeRef<string>, enabled: MaybeRef<boolean> = true) =>
  useQuery({
    queryKey: ['queues-view', brokerId],
    queryFn: async () => {
      const response = await axios.get<QueueViewDto[]>(`${API_URL}/queues/view`, {
        params: {
          brokerId: toValue(brokerId)
        },
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(enabled) && !!toValue(brokerId))
    // refetchInterval: 2000
  })
