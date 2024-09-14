import { API_URL } from '@/constants/api'
import type { ContinueSubscriptionDto } from '@/dtos/continueSubscriptionDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useContinueSubscriptionMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: ContinueSubscriptionDto) =>
      axios.post(`${API_URL}/stripe/continue-subscription`, data, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['me'] })
    }
  })
}
