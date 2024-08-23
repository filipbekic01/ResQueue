import axios from 'axios'
import { useMutation } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'
import type { ResetPasswordDto } from '@/dtos/auth/resetPasswordDto'

export function useResetPasswordMutation() {
  return useMutation({
    mutationFn: (data: ResetPasswordDto) =>
      axios.post(`${API_URL}/resetPassword`, data, {
        withCredentials: true
      })
  })
}
