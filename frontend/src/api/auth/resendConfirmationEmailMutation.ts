import { API_URL } from '@/constants/api'
import type { ResendConfirmationEmailDto } from '@/dtos/auth/resendConfirmationEmailDto'
import { useMutation } from '@tanstack/vue-query'
import axios from 'axios'

export function useResendConfirmatioEmailMutation() {
  return useMutation({
    mutationFn: (data: ResendConfirmationEmailDto) =>
      axios.post(`${API_URL}/resendConfirmationEmail`, data, {
        withCredentials: true
      })
  })
}
