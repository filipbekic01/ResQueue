import { API_URL } from '@/constants/api'
import type { MoveMessagesDto } from '@/dtos/message/moveMessagesDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

interface MoveMessagesResponse {
  succeededCount: number
}

export function useMoveMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dto: MoveMessagesDto) =>
      axios
        .post<MoveMessagesResponse>(`${API_URL}/messages/move`, dto, {
          withCredentials: true
        })
        .then((x) => x.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages/paginated'] })
      queryClient.invalidateQueries({ queryKey: ['messages'] })
    }
  })
}
