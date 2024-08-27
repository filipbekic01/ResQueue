import { API_URL } from '@/constants/api'
import type { UpdateBrokerDto } from '@/dtos/updateBrokerDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export interface UpdateBrokerRequest {
  brokerId: string
  broker: UpdateBrokerDto
}

export function useUpdateBrokerMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: UpdateBrokerRequest) =>
      axios.put(`${API_URL}/brokers/${request.brokerId}`, request.broker, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['brokers'] })
    }
  })
}
