import { API_URL } from '@/constants/api'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useArchiveMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: { ids: string[]; queueId: string; purge: boolean }) =>
      axios.delete(`${API_URL}/messages`, {
        data: {
          ids: data.ids,
          queueId: data.queueId
        },
        withCredentials: true,
        params: {
          purge: data.purge ? data.purge : undefined
        }
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages/paginated'] })
      queryClient.invalidateQueries({ queryKey: ['messages'] })
      queryClient.invalidateQueries({ queryKey: ['queues'] })
      queryClient.invalidateQueries({ queryKey: ['queue'] })
    }
  })
}
