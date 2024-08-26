import axios from 'axios'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'
import type { CancelSubscriptionDto } from '@/dtos/cancelSubscriptionDto'

export function useCancelSubscriptionMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CancelSubscriptionDto) =>
      axios.post(`${API_URL}/stripe/cancel-subscription`, data, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['me'] })
    }
  })
}
