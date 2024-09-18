import { API_URL } from '@/constants/api'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useContinueSubscriptionMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () =>
      axios.post(
        `${API_URL}/stripe/continue-subscription`,
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
