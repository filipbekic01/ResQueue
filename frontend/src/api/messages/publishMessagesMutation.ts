import axios from 'axios'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'

export interface PublishRequest {
  exchangeId: string
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
      queryClient.invalidateQueries({ queryKey: ['messages'] }) // for specific key please, check other places too
    }
  })
}
