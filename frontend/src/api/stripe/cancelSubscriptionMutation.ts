import axios from 'axios'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'

export function useCancelSubscriptionMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () =>
      axios.post(
        `${API_URL}/stripe/cancel-subscription`,
        {},
        {
          withCredentials: true
        }
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['me'] })
    }
  })
}
