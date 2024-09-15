import { API_URL } from '@/constants/api'
import type { BrokerInvitationDto } from '@/dtos/broker/brokerInvitationDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useBrokerInvitationsQuery = (id: MaybeRef<string | undefined>) =>
  useQuery({
    queryKey: ['broker-invitations', id],
    queryFn: async () => {
      const response = await axios.get<BrokerInvitationDto[]>(`${API_URL}/brokers/invitations`, {
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(id))
  })
