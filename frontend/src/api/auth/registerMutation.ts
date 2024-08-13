import axios from 'axios'
import { useMutation } from '@tanstack/vue-query'
import { API_URL } from '@/constants/api'

export interface RegisterRequest {
  email: string
  password: string
}

export interface RegisterResponse {}

export function useRegisterMutation() {
  return useMutation({
    mutationFn: (data: RegisterRequest) =>
      axios.post(`${API_URL}/register`, data, {
        withCredentials: true
      })
  })
}
