import { API_URL } from '@/constants/api'
import type { BrokerInvitationDto } from '@/dtos/broker/brokerInvitationDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { toValue, type MaybeRef } from 'vue'

export const useBrokerInvitationsQuery = (brokerId?: MaybeRef<string | undefined>) =>
  useQuery({
    queryKey: ['broker-invitations', brokerId],
    queryFn: async () => {
      const response = await axios.get<BrokerInvitationDto[]>(`${API_URL}/brokers/invitations`, {
        params: {
          brokerId: brokerId ? toValue(brokerId) : undefined
        },
        withCredentials: true
      })

      return response.data
    }
  })
