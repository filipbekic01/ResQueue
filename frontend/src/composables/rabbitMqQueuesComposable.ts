import type { QueueDto } from '@/dtos/queueDto'
import type { RabbitMqQueueDto } from '@/dtos/rabbitMqQueueDto'
import { computed, type Ref } from 'vue'

export function useRabbitMqQueues(queues: Ref<QueueDto[]> | Ref<undefined>) {
  const rabbitMqQueues = computed<RabbitMqQueueDto[]>(
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
