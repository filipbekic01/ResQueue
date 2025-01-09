import { API_URL } from '@/constants/api'
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import type { PaginatedResult } from '@/dtos/paginatedResultDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useMessagesQuery = (
  queueId: MaybeRef<number | undefined>,
  pageIndex: MaybeRef<number>,
  refetchInterval: MaybeRef<number> = 5000,
) =>
  useQuery({
    queryKey: ['messages', queueId, pageIndex],
    queryFn: async () => {
      const response = await axios.get<PaginatedResult<MessageDeliveryDto>>(`${API_URL}/messages`, {
        params: {
          queueId: toValue(queueId),
          pageSize: 50,
          pageIndex: toValue(pageIndex),
        },
        withCredentials: true,
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
          if (x.message.headers) {
            x.message.headers = JSON.parse(x.message.headers ?? '{}')
          }
        } catch {
          x.message.headers = {}
        }

        try {
          const body = JSON.parse(x.message.body)
          if (body['jobId']) {
            x.isRecurring = true
          } else {
            x.isRecurring = false
          }
        } catch (e) {
          console.error(e)
          x.isRecurring = false
        }
      })

      return response.data
    },
    enabled: computed(() => !!toValue(queueId)),
    refetchInterval: computed(() => toValue(refetchInterval)),
  })
