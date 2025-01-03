import { API_URL } from '@/constants/api'
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { type MaybeRef } from 'vue';

export const useSingleMessageQuery = (
  transportMessageId: MaybeRef<string>
) =>
  useQuery({
    queryKey: ['singleMessage', transportMessageId],
    queryFn: async () => {
      const response = await axios.get<MessageDeliveryDto>(`${API_URL}/messages/${transportMessageId}`, {
        withCredentials: true
      })

      const data = response.data;

      if (data) {
        try {
          data.transportHeaders = JSON.parse(data.transportHeaders ?? '{}')
        } catch {
          data.transportHeaders = {}
        }

        try {
          data.message.host = JSON.parse(data.message.host ?? '{}')
        } catch {
          data.message.host = {}
        }

        try {
          if (data.message.headers) {
            data.message.headers = JSON.parse(data.message.headers ?? '{}')
          }
        } catch {
          data.message.headers = {}
        }
      }

      return data
    }
  })
