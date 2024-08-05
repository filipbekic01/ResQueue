import axios from 'axios'
import { useMutation } from '@tanstack/vue-query'

export function useLogoutMutation() {
  return useMutation({
    mutationFn: () =>
      axios.post(
        'http://localhost:5182/auth/logout',
        {},
        {
          withCredentials: true
        }
      )
  })
}
