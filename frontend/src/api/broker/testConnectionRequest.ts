import { API_URL } from '@/constants/api'
import type { CreateBrokerDto } from '@/dtos/createBrokerDto'
import { useMutation } from '@tanstack/vue-query'
import axios from 'axios'

export function useTestConnectionMutation() {
  return useMutation({
    mutationFn: (broker: CreateBrokerDto) =>
      axios.post(`${API_URL}/brokers/test-connection`, broker, {
        withCredentials: true
      })
  })
}
