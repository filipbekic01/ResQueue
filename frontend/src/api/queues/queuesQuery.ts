import { API_URL } from '@/constants/api'
import type { PaginatedResult } from '@/dtos/paginatedResultDto'
import type { QueueDto } from '@/dtos/queueDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useQueuesQuery = (
  brokerId: MaybeRef<string | undefined>,
  pageIndex: MaybeRef<number>
) =>
  useQuery({
    queryKey: ['queues', brokerId, pageIndex],
    queryFn: async () => {
      const response = await axios.get<PaginatedResult<QueueDto>>(`${API_URL}/queues`, {
        params: {
          brokerId: toValue(brokerId),
          pageSize: 50,
          pageIndex: toValue(pageIndex)
        },
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(brokerId))
  })
