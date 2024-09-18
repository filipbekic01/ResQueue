import { API_URL } from '@/constants/api'
import type { SubscribeDto } from '@/dtos/users/subscribeDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

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
