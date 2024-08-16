import { API_URL } from '@/constants/api'
import type { QueueDto } from '@/dtos/queueDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useQueueQuery = (id: MaybeRef<string | undefined>) =>
  useQuery({
    queryKey: ['queues', id],
    queryFn: async () => {
      const response = await axios.get<QueueDto[]>(`${API_URL}/queues`, {
        params: {
          ids: [toValue(id), '66bf0c4873456a33536ab211']
        },
        withCredentials: true
      })

      return response.data[0] ?? undefined
    },
    enabled: computed(() => !!toValue(id))
  })
