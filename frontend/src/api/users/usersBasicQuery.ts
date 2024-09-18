import { API_URL } from '@/constants/api'
import type { UserBasicDto } from '@/dtos/users/userBasicDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useUsersBasicQuery = (ids: MaybeRef<string[] | undefined>) =>
  useQuery({
    queryKey: ['users-basic', ids],
    queryFn: async () => {
      const response = await axios.get<UserBasicDto[]>(`${API_URL}/users/basic`, {
        params: {
          ids: toValue(ids)
        },
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(ids))
  })
