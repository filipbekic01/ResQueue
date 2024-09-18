import { API_URL } from '@/constants/api'
import type { FavoriteQueueDto } from '@/dtos/queues/favoriteQueueDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export interface FavoriteQueueMutationDto {
  queueId: string
  dto: FavoriteQueueDto
}

export function useFavoriteQueueMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: FavoriteQueueMutationDto) =>
      axios.post(`${API_URL}/queues/${data.queueId}/favorite`, data.dto, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['queues'] }) // for specific key please, check other places too
    }
  })
}
