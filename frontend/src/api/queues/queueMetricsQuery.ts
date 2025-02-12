import { API_URL } from '@/constants/api'
import type { QueueMetricDto } from '@/dtos/queue/queueMetricDto'
import { useQuery } from '@tanstack/vue-query'
import axios from 'axios'
import { computed, toValue, type MaybeRef } from 'vue'

export const useQueueMetricsQuery = (
  queueId: MaybeRef<number>,
  refetchInterval: MaybeRef<number> = 5000,
) =>
  useQuery({
    queryKey: ['queue-metrics', queueId],
    queryFn: async () => {
      const response = await axios.get<QueueMetricDto[]>(
        `${API_URL}/queues/${toValue(queueId)}/metrics`,
        {
          withCredentials: true,
        },
      )

      return response.data
    },
    enabled: computed(() => !!toValue(queueId)),
    refetchInterval: refetchInterval,
  })
