import { API_URL } from '@/constants/api'
import type { CreateBrokerInvitationDto } from '@/dtos/broker/createBrokerInvitationDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useCreateBrokerInvitationMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: CreateBrokerInvitationDto) =>
      axios.post(`${API_URL}/brokers/invitations`, request, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['broker-invitations'] })
    }
  })
}
