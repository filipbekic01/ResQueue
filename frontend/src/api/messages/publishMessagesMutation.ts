import { API_URL } from '@/constants/api'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export interface PublishRequest {
  exchangeId: string
  queueId: string
  brokerId: string
  messageIds: string[]
}

export function usePublishMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: PublishRequest) =>
      axios.post(`${API_URL}/messages/publish`, request, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages/paginated'] })
    }
  })
}
