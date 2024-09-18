import { API_URL } from '@/constants/api'
import type { QueueDto } from '@/dtos/queues/queueDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useQueueQuery = (id: MaybeRef<string | undefined>) =>
  useQuery({
    queryKey: ['queue', id],
    queryFn: async () => {
      const response = await axios.get<QueueDto[]>(`${API_URL}/queues`, {
        params: {
          ids: [toValue(id)]
        },
        withCredentials: true
      })

      return response.data[0] ?? undefined
    },
    enabled: computed(() => !!toValue(id))
  })
