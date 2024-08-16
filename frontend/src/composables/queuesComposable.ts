import type { QueueDto } from '@/dtos/queueDto'
import { computed, type Ref } from 'vue'

export function useRabbitMqQueues(queues: Ref<QueueDto[] | undefined>) {
  const rabbitMqQueues = computed(
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
