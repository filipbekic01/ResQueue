import { API_URL } from '@/constants/api'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useUpdateSeatsMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: { seats: number }) =>
      axios.post(`${API_URL}/stripe/update-seats`, data, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['me'] })
    }
  })
}
