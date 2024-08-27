import { API_URL } from '@/constants/api'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useSyncMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (queueId: string) =>
      axios.post(
        `${API_URL}/messages/sync`,
        {},
        {
          params: {
            queueId
          },
          withCredentials: true
        }
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages'] }) // for specific key please, check other places too
    }
  })
}
