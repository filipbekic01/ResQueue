import { API_URL } from '@/constants/api'
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import type { PaginatedResult } from '@/dtos/paginatedResultDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const usePaginatedMessagesQuery = (queueId: MaybeRef<number | undefined>, pageIndex: MaybeRef<number>) =>
  useQuery({
    queryKey: ['messages/paginated', queueId, pageIndex],
    queryFn: async () => {
      const response = await axios.get<PaginatedResult<MessageDeliveryDto>>(`${API_URL}/messages/paginated`, {
        params: {
          queueId: toValue(queueId),
          pageSize: 50,
          pageIndex: toValue(pageIndex)
        },
        withCredentials: true
      })

      response.data.items.forEach((x) => {
        try {
          x.transport_headers = JSON.parse(x.transport_headers ?? '{}')
        } catch {
          x.transport_headers = {}
        }

        try {
          x.message.host = JSON.parse(x.message.host ?? '{}')
        } catch {
          x.message.host = {}
        }
      })

      return response.data
    },
    enabled: computed(() => !!toValue(queueId))
  })
