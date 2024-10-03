import { API_URL } from '@/constants/api'
import type { SyncMessagesDto } from '@/dtos/message/syncMessagesDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useSyncMessagesMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: SyncMessagesDto) =>
      axios.post(`${API_URL}/messages/sync`, data, {
        withCredentials: true
      }),
    onSuccess: (_, { queueId }) => {
      queryClient.invalidateQueries({ queryKey: ['messages/paginated'] })
      queryClient.invalidateQueries({ queryKey: ['queues'] })
      queryClient.invalidateQueries({ queryKey: ['queue', queueId] })
    }
  })
}
