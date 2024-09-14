import { API_URL } from '@/constants/api'
import type { CreateMessageDto } from '@/dtos/createMessageDto'
import { useMutation } from '@tanstack/vue-query'
import axios from 'axios'

export function useCreateMessageMutation() {
  return useMutation({
    mutationFn: (request: CreateMessageDto) =>
      axios.post(`${API_URL}/messages`, request, {
        withCredentials: true
      })
  })
}
