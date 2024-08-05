import axios from 'axios'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import type { CreateBrokerDto } from '@/dtos/createBrokerDto'
import { API_URL } from '@/constants/Api'

export function useCreateBrokerMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (broker: CreateBrokerDto) =>
      axios.post(`${API_URL}/brokers`, broker, {
        withCredentials: true
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['brokers'] })
    }
  })
}
