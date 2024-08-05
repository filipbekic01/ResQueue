import axios from 'axios'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { API_URL } from '@/constants/Api'

export function useDeleteBrokerMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (brokerId: string) =>
      axios.delete(`${API_URL}/brokers/${brokerId}`, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['brokers'] })
    }
  })
}
