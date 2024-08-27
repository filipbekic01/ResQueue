import { API_URL } from '@/constants/api'
import type { BrokerDto } from '@/dtos/brokerDto'
import type { CreateBrokerDto } from '@/dtos/createBrokerDto'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import axios from 'axios'

export function useCreateBrokerMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (broker: CreateBrokerDto) =>
      axios
        .post(`${API_URL}/brokers`, broker, {
          withCredentials: true
        })
        .then((x) => x.data as BrokerDto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['brokers'] })
    }
  })
}
