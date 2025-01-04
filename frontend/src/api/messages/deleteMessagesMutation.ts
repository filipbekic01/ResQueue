import { API_URL } from '@/constants/api'
import type { DeleteMessagesDto } from '@/dtos/message/deleteMessagesDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useDeleteMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dto: DeleteMessagesDto) =>
      axios.delete(`${API_URL}/messages`, {
        data: {
          ...dto,
        },
        withCredentials: true,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages'] })
      queryClient.invalidateQueries({ queryKey: ['queues-view'] })
      queryClient.invalidateQueries({ queryKey: ['queue-view'] })
    },
  })
}
