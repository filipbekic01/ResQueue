import { API_URL } from '@/constants/api'
import type { QueueViewDto } from '@/dtos/queue/queueViewDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useQueuesQuery = (
  brokerId: MaybeRef<string | undefined>,
  pageIndex: MaybeRef<number>,
  search: MaybeRef<string | undefined>,
  sortField: MaybeRef<string | undefined>,
  sortOrder: MaybeRef<string | undefined>,
  enabled: MaybeRef<boolean>
) =>
  useQuery({
    queryKey: ['queues', brokerId, pageIndex, search, sortField, sortOrder],
    queryFn: async () => {
      const response = await axios.get<QueueViewDto[]>(`${API_URL}/queues/paginated`, {
        params: {
          brokerId: toValue(brokerId),
          pageSize: 50,
          pageIndex: toValue(pageIndex),
          search: toValue(search),
          sortField: toValue(sortField),
          sortOrder: toValue(sortOrder)
        },
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(brokerId) && toValue(enabled)),
    refetchInterval: 2000
  })
