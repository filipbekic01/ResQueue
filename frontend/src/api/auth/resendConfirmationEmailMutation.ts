import axios from 'axios'
import { useMutation } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'
import type { ResendConfirmationEmailDto } from '@/dtos/auth/resendConfirmationEmailDto'

export function useResendConfirmatioEmailMutation() {
  return useMutation({
    mutationFn: (data: ResendConfirmationEmailDto) =>
      axios.post(`${API_URL}/resendConfirmationEmail`, data, {
        withCredentials: true
      })
  })
}
