import axios from 'axios'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'

export interface ReviewMessagesRequest {
  idsToTrue: string[]
  idsToFalse: string[]
}

export function useReviewMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: ReviewMessagesRequest) => {
      return axios.post(`${API_URL}/messages/review`, request, {
        withCredentials: true
      })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages'] }) // for specific key please, check other places too
    }
  })
}
