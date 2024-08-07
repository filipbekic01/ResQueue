import axios from 'axios'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { API_URL } from '@/constants/Api'

export function useSyncBrokerMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (brokerId: string) =>
      axios.post(
        `${API_URL}/brokers/${brokerId}/sync`,
        {},
        {
          withCredentials: true
        }
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['queues'] })
      queryClient.invalidateQueries({ queryKey: ['exchanges'] })
      queryClient.invalidateQueries({ queryKey: ['brokers'] })
    }
  })
}
