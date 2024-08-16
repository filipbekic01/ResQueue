import { API_URL } from '@/constants/api'
import type { MessageDto } from '@/dtos/messageDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useMessageQuery = (id: MaybeRef<string | undefined>) =>
  useQuery({
    queryKey: ['messages', id],
    queryFn: async () => {
      const response = await axios.get<MessageDto[]>(`${API_URL}/messages`, {
        params: {
          ids: [toValue(id)]
        },
        withCredentials: true
      })

      return response.data[0] ?? undefined
    },
    enabled: computed(() => !!toValue(id))
  })
