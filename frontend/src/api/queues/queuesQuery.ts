import { API_URL } from '@/constants/api'
import type { QueueDto } from '@/dtos/queue/queueDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useQueuesQuery = (queueName: MaybeRef<string>, refetchInterval: MaybeRef<number> = 5000) =>
  useQuery({
    queryKey: ['queues', queueName],
    queryFn: async () => {
      const response = await axios.get<QueueDto[]>(`${API_URL}/queues`, {
        params: {
          queueName: toValue(queueName)
        },
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(queueName)),
    refetchInterval: refetchInterval
  })
