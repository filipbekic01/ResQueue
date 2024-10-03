import { API_URL } from '@/constants/api'
import type { MessageDto } from '@/dtos/message/messageDto'
import type { PaginatedResult } from '@/dtos/paginatedResultDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const usePaginatedMessagesQuery = (
  brokerId: MaybeRef<string | undefined>,
  queueId: MaybeRef<string | undefined>,
  pageIndex: MaybeRef<number>
) =>
  useQuery({
    queryKey: ['messages/paginated', brokerId, queueId, pageIndex],
    queryFn: async () => {
      const response = await axios.get<PaginatedResult<MessageDto>>(`${API_URL}/messages/paginated`, {
        params: {
          brokerId: toValue(brokerId),
          queueId: toValue(queueId),
          pageSize: 50,
          pageIndex: toValue(pageIndex)
        },
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(brokerId) && !!toValue(queueId))
  })
