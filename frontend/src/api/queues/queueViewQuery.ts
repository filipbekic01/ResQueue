import { API_URL } from '@/constants/api'
import type { QueueViewDto } from '@/dtos/queue/queueViewDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useQueueViewQuery = (queueName: MaybeRef<string>, enabled: MaybeRef<boolean> = true) =>
  useQuery({
    queryKey: ['queue-view', queueName],
    queryFn: async () => {
      const response = await axios.get<QueueViewDto>(`${API_URL}/queues/view/${toValue(queueName)}`, {
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(enabled) && !!toValue(queueName)),
    refetchInterval: 3000
  })
