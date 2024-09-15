import { API_URL } from '@/constants/api'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useCloneMessageMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) =>
      axios.post(
        `${API_URL}/messages/${id}/clone`,
        {},
        {
          withCredentials: true
        }
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages'] })
    }
  })
}
