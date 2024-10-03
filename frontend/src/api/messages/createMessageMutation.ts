import { API_URL } from '@/constants/api'
import type { UpsertMessageDto } from '@/dtos/message/upsertMessageDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useCreateMessageMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: UpsertMessageDto) =>
      axios.post(`${API_URL}/messages`, request, {
        withCredentials: true
      }),
    onSuccess: (_, request) => {
      queryClient.invalidateQueries({ queryKey: ['messages/paginated'] })
      queryClient.invalidateQueries({ queryKey: ['queue', request.queueId] })
      queryClient.invalidateQueries({ queryKey: ['queues'] })
    }
  })
}
