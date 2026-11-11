import { useQuery } from "@tanstack/vue-query";
import axios from "axios";
import { computed, toValue, type MaybeRef } from "vue";
import { API_URL } from "@/constants/api";
import type { QueueDepthHistoryDto } from "@/dtos/queue/queueDepthMetricDto";

export const useQueueDepthMetricsQuery = (queueId: MaybeRef<number>, refetchInterval: MaybeRef<number> = 5000) =>
  useQuery({
    queryKey: ["queue-depth-metrics", queueId],
    queryFn: async () => {
      const response = await axios.get<QueueDepthHistoryDto>(`${API_URL}/queues/${toValue(queueId)}/depth-metrics`, {
        withCredentials: true,
      });

      return response.data;
    },
    enabled: computed(() => !!toValue(queueId)),
    refetchInterval: refetchInterval,
  });
