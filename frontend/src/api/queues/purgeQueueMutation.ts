import { API_URL } from '@/constants/api'
import type { PurgeQueueDto } from '@/dtos/queue/purgeQueueDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function usePurgeQueueMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dto: PurgeQueueDto) =>
      axios.post(`${API_URL}/queues/purge`, dto, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages'] })
      queryClient.invalidateQueries({ queryKey: ['queues-view'] })
      queryClient.invalidateQueries({ queryKey: ['queue-view'] })
    }
  })
}
