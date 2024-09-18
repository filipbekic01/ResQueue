import { API_URL } from '@/constants/api'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export interface ExpireBrokerInvitationRequest {
  id: string
}

export function useExpireBrokerInvitationMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: ExpireBrokerInvitationRequest) =>
      axios.post(
        `${API_URL}/brokers/invitations/${request.id}/expire`,
        {},
        {
          withCredentials: true
        }
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['broker-invitations'] })
    }
  })
}
