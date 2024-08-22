import axios from 'axios'
import { useMutation } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'
import type { RegisterDto } from '@/dtos/registerDto'

export function useRegisterMutation() {
  return useMutation({
    mutationFn: (data: RegisterDto) =>
      axios.post(`${API_URL}/auth/register`, data, {
        withCredentials: true
      })
  })
}
