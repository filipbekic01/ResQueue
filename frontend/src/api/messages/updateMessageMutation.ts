import { API_URL } from '@/constants/api'
import type { UpsertMessageDto } from '@/dtos/message/upsertMessageDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useUpdateMessageMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: UpsertMessageDto }) =>
      axios.put(`${API_URL}/messages/${id}`, dto, {
        withCredentials: true
      }),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['message', id] })
      queryClient.invalidateQueries({ queryKey: ['messages'] })
    }
  })
}
