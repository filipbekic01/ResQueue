import { API_URL } from '@/constants/api'
import type { MoveMessagesDto } from '@/dtos/message/moveMessagesDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useMoveMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dto: MoveMessagesDto) =>
      axios.post(`${API_URL}/messages/move`, dto, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages/paginated'] })
      queryClient.invalidateQueries({ queryKey: ['messages'] })
    }
  })
}
