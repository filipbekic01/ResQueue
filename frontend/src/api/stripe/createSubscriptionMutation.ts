import axios from 'axios'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'

export function useCreateSubscriptionMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (ids: string[]) =>
      axios.delete(`${API_URL}/stripe/create-subscription`, {
        data: {
          ids
        },
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages'] }) // for specific key please, check other places too
    }
  })
}
