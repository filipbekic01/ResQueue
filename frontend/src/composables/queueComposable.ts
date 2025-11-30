import { computed, toValue, type Ref } from "vue";
import { useQueuesQuery } from "@/api/queues/queuesQuery";
import { useQueueViewQuery } from "@/api/queues/queueViewQuery";
import type { QueueViewDto } from "@/dtos/queue/queueViewDto";
import { QueueTypeEnum } from "@/enums/queueTypeEnum";
import { useLocalSettings } from "./useLocalSettings";

const getQueueName = (type: number, queueView?: QueueViewDto) => {
  if (type === 1) {
    return `Ready (${(queueView?.ready ?? 0) + (queueView?.scheduled ?? 0)})`;
  } else if (type === 2) {
    return `Error (${queueView?.errored})`;
  } else if (type === 3) {
    return `Dead-Letter (${queueView?.deadLettered})`;
  } else {
    return "Unknown";
  }
};

const getQueueTypeLabel = (type?: number) => {
  if (type === 1) {
    return `ready`;
  } else if (type === 2) {
    return `error`;
  } else if (type === 3) {
    return `dead-letter`;
  } else {
    return "Unknown";
  }
};

export function useQueue(queueName: Ref<string>) {
  const { refetchInterval } = useLocalSettings();

  const query = useQueuesQuery(queueName, refetchInterval);
  const queryView = useQueueViewQuery(queueName, refetchInterval);

  const queueOptions = computed(() => {
    if (!query.data.value) {
      return [];
    }

    return [...query.data.value]
      .sort((a, b) => a.type - b.type)
      .map((queue) => ({
        queueNameByType: getQueueName(queue.type, toValue(queryView.data)),
        queue: queue,
      }));
  });

  const primaryQueue = computed(() => query.data.value?.find((x) => x.type === QueueTypeEnum.PRIMARY));

  return {
    query,
    queryView,
    queueOptions,
    primaryQueue,
    getQueueTypeLabel,
  };
}
