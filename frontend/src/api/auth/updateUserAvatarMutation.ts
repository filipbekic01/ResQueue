import { API_URL } from '@/constants/api'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useUpdateUserAvatarMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () =>
      axios.patch(
        `${API_URL}/auth/me/avatar`,
        {},
        {
          withCredentials: true
        }
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['me'] })
      queryClient.invalidateQueries({ queryKey: ['users'] })
    }
  })
}
