import { API_URL } from '@/constants/api'
import type { MessageDto } from '@/dtos/message/messageDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useMessagesQuery = (queueId: MaybeRef<string | undefined>, ids: MaybeRef<string[] | undefined>) =>
  useQuery({
    queryKey: ['messages', ids],
    queryFn: async () => {
      const response = await axios.get<MessageDto[]>(`${API_URL}/messages`, {
        params: {
          queueId: toValue(queueId),
          ids: toValue(ids)
        },
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(queueId) && !!toValue(ids)?.length)
  })
