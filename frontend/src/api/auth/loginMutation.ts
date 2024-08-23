import axios from 'axios'
import { useMutation } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'
import type { LoginDto } from '@/dtos/auth/loginDto'

export function useLoginMutation() {
  return useMutation({
    mutationFn: (data: LoginDto) =>
      axios.post(`${API_URL}/login?useCookies=true`, data, {
        withCredentials: true
      })
  })
}
