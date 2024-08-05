import { API_URL } from '@/constants/Api'
import type { QueueDto } from '@/dtos/queueDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useQueuesQuery = (brokerId: MaybeRef<string | undefined>) =>
  useQuery({
    queryKey: ['queues', brokerId],
    queryFn: async () => {
      const response = await axios.get<QueueDto[]>(`${API_URL}/queues/${toValue(brokerId)}`, {
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(brokerId))
  })
