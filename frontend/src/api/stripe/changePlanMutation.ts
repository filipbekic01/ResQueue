import { API_URL } from '@/constants/api'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useChangePlanMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () =>
      axios.post(
        `${API_URL}/stripe/change-plan`,
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
