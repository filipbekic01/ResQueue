import { API_URL } from '@/constants/api'
import type { BrokerInvitationDto } from '@/dtos/broker/brokerInvitationDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useBrokerInvitationQuery = (token: MaybeRef<string | undefined>) =>
  useQuery({
    queryKey: ['broker-invitation', token],
    queryFn: async () => {
      const response = await axios.get<BrokerInvitationDto>(
        `${API_URL}/brokers/invitations/${toValue(token)}`,
        {
          withCredentials: true
        }
      )

      return response.data
    },
    enabled: computed(() => !!toValue(token))
  })
