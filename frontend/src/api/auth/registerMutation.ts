import { API_URL } from '@/constants/api'
import type { RegisterDto } from '@/dtos/user/registerDto'
import { useMutation } from '@tanstack/vue-query'
import axios from 'axios'

export function useRegisterMutation() {
  return useMutation({
    mutationFn: (data: RegisterDto) =>
      axios.post(`${API_URL}/auth/register`, data, {
        withCredentials: true
      })
  })
}
