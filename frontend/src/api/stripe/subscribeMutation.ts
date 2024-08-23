import axios from 'axios'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'
import type { SubscribeDto } from '@/dtos/subscribeDto'

export function useSubscribeMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: SubscribeDto) =>
      axios.post(`${API_URL}/stripe/subscribe`, data, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['me'] })
    }
  })
}
