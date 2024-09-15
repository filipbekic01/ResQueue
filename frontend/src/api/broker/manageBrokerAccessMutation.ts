import { API_URL } from '@/constants/api'
import type { ManageBrokerAccessDto } from '@/dtos/broker/manageBrokerAccessDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useManageBrokerAccessMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: ManageBrokerAccessDto) =>
      axios.post(`${API_URL}/brokers/access`, request, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['brokers'] })
    }
  })
}
