import { API_URL } from '@/constants/api'
import type { RequeueSpecificMessagesDto } from '@/dtos/message/requeueSpecificMessagesDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useRequeueSpecificMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dto: RequeueSpecificMessagesDto) =>
      axios.post(`${API_URL}/messages/requeue-specific`, dto, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages'] })
      queryClient.invalidateQueries({ queryKey: ['queues-view'] })
      queryClient.invalidateQueries({ queryKey: ['queue-view'] })
    }
  })
}
