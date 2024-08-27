import { API_URL } from '@/constants/api'
import type { ForgotPasswordDto } from '@/dtos/auth/forgotPasswordDto'
import { useMutation } from '@tanstack/vue-query'
import axios from 'axios'

export function useForgotPasswordMutation() {
  return useMutation({
    mutationFn: (data: ForgotPasswordDto) =>
      axios.post(`${API_URL}/forgotPassword`, data, {
        withCredentials: true
      })
  })
}
