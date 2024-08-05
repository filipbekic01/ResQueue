import axios from 'axios'
import { useMutation } from '@tanstack/vue-query'

export interface RegisterRequest {
  email: string
  password: string
}

export interface RegisterResponse {}

export function useRegisterMutation() {
  return useMutation({
    mutationFn: (data: RegisterRequest) =>
      axios.post('http://localhost:5182/register', data, {
        withCredentials: true
      })
  })
}
