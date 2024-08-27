import { API_URL } from '@/constants/api'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useArchiveMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (ids: string[]) =>
      axios.delete(`${API_URL}/messages`, {
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
