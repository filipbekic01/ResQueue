import axios from 'axios'
import { useMutation } from '@tanstack/vue-query'

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {}

export function useLoginMutation() {
  return useMutation({
    mutationFn: (data: LoginRequest) =>
      axios.post('http://localhost:5182/login?useCookies=true', data, {
        withCredentials: true
      })
  })
}
