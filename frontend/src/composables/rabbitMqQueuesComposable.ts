import type { QueueDto } from '@/dtos/queues/queueDto'
import type { RabbitMQQueueDto } from '@/dtos/queues/rabbitMQQueueDto'
import { computed, type Ref } from 'vue'

export function useRabbitMqQueues(queues: Ref<QueueDto[]> | Ref<undefined>) {
  const rabbitMqQueues = computed<RabbitMQQueueDto[]>(
    () =>
      queues.value?.map((q) => {
        const item = {
          ...q,
          parsed: JSON.parse(q.rawData)
        }

        return item
      }) ?? []
  )

  return {
    rabbitMqQueues
  }
}
