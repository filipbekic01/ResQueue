import axios from 'axios'
import { useMutation } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'
import type { ForgotPasswordDto } from '@/dtos/auth/forgotPasswordDto'

export function useForgotPasswordMutation() {
  return useMutation({
    mutationFn: (data: ForgotPasswordDto) =>
      axios.post(`${API_URL}/forgotPassword`, data, {
        withCredentials: true
      })
  })
}
