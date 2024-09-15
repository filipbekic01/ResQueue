import { API_URL } from '@/constants/api'
import type { AcceptBrokerInvitationDto } from '@/dtos/broker/acceptBrokerInvitationDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useAcceptBrokerInvitationMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: AcceptBrokerInvitationDto) =>
      axios.post(`${API_URL}/brokers/invitations/accept`, request, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['broker-invitations'] })
      queryClient.invalidateQueries({ queryKey: ['brokers'] })
    }
  })
}
