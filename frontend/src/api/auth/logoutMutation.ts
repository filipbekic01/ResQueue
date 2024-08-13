import axios from 'axios'
import { useMutation } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'

export function useLogoutMutation() {
  return useMutation({
    mutationFn: () =>
      axios.post(
        `${API_URL}/auth/logout`,
        {},
        {
          withCredentials: true
        }
      )
  })
}
