import { API_URL } from '@/constants/api'
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { type MaybeRef } from 'vue'

export const useTransformMessageQuery = (
  message: MaybeRef<MessageDeliveryDto>
) =>
  useQuery({
    queryKey: ['transform', message],
    queryFn: async () => {
      const response = await axios.post<MessageDeliveryDto>(`${API_URL}/messages/transform`, message,{
        withCredentials: true
      })

      return response.data
    }
  })
