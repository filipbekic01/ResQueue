import { API_URL } from '@/constants/api'
import type { RequeueMessagesDto } from '@/dtos/message/requeueMessagesDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useRequeueMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dto: RequeueMessagesDto) =>
      axios.post(`${API_URL}/messages/requeue`, dto, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages/paginated'] })
      queryClient.invalidateQueries({ queryKey: ['messages'] })
    }
  })
}
