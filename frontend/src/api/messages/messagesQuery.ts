import { API_URL } from '@/constants/api'
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import type { PaginatedResult } from '@/dtos/paginatedResultDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useMessagesQuery = (queueId: MaybeRef<number | undefined>, pageIndex: MaybeRef<number>) =>
  useQuery({
    queryKey: ['messages', queueId, pageIndex],
    queryFn: async () => {
      const response = await axios.get<PaginatedResult<MessageDeliveryDto>>(`${API_URL}/messages`, {
        params: {
          queueId: toValue(queueId),
          pageSize: 50,
          pageIndex: toValue(pageIndex)
        },
        withCredentials: true
      })

      response.data.items.forEach((x) => {
        try {
          x.transportHeaders = JSON.parse(x.transportHeaders ?? '{}')
        } catch {
          x.transportHeaders = {}
        }

        try {
          x.message.host = JSON.parse(x.message.host ?? '{}')
        } catch {
          x.message.host = {}
        }

        try {
          x.message.headers = JSON.parse(x.message.headers ?? '{}')
        } catch {
          x.message.headers = {}
        }
      })

      return response.data
    },
    enabled: computed(() => !!toValue(queueId))
  })
