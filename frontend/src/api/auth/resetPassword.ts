import { API_URL } from '@/constants/api'
import type { ResetPasswordDto } from '@/dtos/auth/resetPasswordDto'
import { useMutation } from '@tanstack/vue-query'
import axios from 'axios'

export function useResetPasswordMutation() {
  return useMutation({
    mutationFn: (data: ResetPasswordDto) =>
      axios.post(`${API_URL}/resetPassword`, data, {
        withCredentials: true
      })
  })
}
