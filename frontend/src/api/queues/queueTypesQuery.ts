import { API_URL } from '@/constants/api'
import type { QueueDto } from '@/dtos/queue/queueDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useQueueTypesQuery = (queueName: MaybeRef<string>) =>
  useQuery({
    queryKey: ['queue-types', queueName],
    queryFn: async () => {
      const response = await axios.get<QueueDto[]>(`${API_URL}/queues/types`, {
        params: {
          queueName: toValue(queueName)
        },
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(queueName))
  })
