import { API_URL } from '@/constants/api'
import type { JobStateDto } from '@/dtos/job/jobStateDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useJobStateQuery = (jobId: MaybeRef<string | undefined>) =>
  useQuery({
    queryKey: ['job-state', jobId],
    queryFn: async () => {
      const response = await axios.get<JobStateDto>(`${API_URL}/jobs/${toValue(jobId)}/state`, {
        params: {
          jobId: toValue(jobId)
        },
        withCredentials: true
      })

      return response.data
    },
    enabled: computed(() => !!toValue(jobId))
  })
