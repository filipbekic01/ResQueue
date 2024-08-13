import axios from 'axios'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'

export function useSyncMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (queueId: string) =>
      axios.post(
        `${API_URL}/messages/${queueId}/sync`,
        {},
        {
          withCredentials: true
        }
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages'] }) // for specific key please, check other places too
    }
  })
}
