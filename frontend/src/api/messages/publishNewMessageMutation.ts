import { API_URL } from '@/constants/api'
import type { NewMessageDto } from '@/dtos/newMessageDto'
import { useMutation } from '@tanstack/vue-query'
import axios from 'axios'

export function usePublishNewMessageMutation() {
  return useMutation({
    mutationFn: (request: NewMessageDto) =>
      axios.post(`${API_URL}/messages/publish-new`, request, {
        withCredentials: true
      })
  })
}
