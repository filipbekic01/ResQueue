import { API_URL } from '@/constants/api'
import type { ChangeCardDto } from '@/dtos/stripe/changeCardDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useChangeCardMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: ChangeCardDto) =>
      axios.post(`${API_URL}/stripe/change-card`, data, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['me'] })
    }
  })
}
