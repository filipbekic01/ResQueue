import { API_URL } from '@/constants/api'
import { useMutation } from '@tanstack/vue-query'
import axios from 'axios'

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
