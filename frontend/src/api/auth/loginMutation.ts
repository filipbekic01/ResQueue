import { API_URL } from '@/constants/api'
import type { LoginDto } from '@/dtos/auth/loginDto'
import { useMutation } from '@tanstack/vue-query'
import axios from 'axios'

export function useLoginMutation() {
  return useMutation({
    mutationFn: (data: LoginDto) =>
      axios.post(`${API_URL}/login?useCookies=true`, data, {
        withCredentials: true
      })
  })
}
