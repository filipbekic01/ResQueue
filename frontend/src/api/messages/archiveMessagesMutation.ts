import { API_URL } from '@/constants/api'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useArchiveMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: { ids: string[]; queueId: string }) =>
      axios.delete(`${API_URL}/messages`, {
        data,
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages/paginated'] })
      queryClient.invalidateQueries({ queryKey: ['messages'] })
      queryClient.invalidateQueries({ queryKey: ['queues'] })
      queryClient.invalidateQueries({ queryKey: ['queue'] })
    }
  })
}
