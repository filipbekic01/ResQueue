import axios from 'axios'
import { useMutation } from '@tanstack/vue-query'
import { API_URL } from '@/constants/Api'

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {}

export function useLoginMutation() {
  return useMutation({
    mutationFn: (data: LoginRequest) =>
      axios.post(`${API_URL}/login?useCookies=true`, data, {
        withCredentials: true
      })
  })
}
