import { API_URL } from '@/constants/Api'
import type { MessageDto } from '@/dtos/messageDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useMessagesQuery = (queueId: MaybeRef<string | undefined>) =>
  useQuery({
    queryKey: ['messages', queueId],
    queryFn: async () => {
      const response = await axios.get<MessageDto[]>(`${API_URL}/messages/${toValue(queueId)}`, {
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(queueId))
  })
