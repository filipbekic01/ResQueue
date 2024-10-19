import { useQueuesQuery } from '@/api/queues/queuesQuery'
import { computed, type Ref } from 'vue'

const getQueueName = (type: number) => {
  if (type === 1) {
    return 'Active Messages'
  } else if (type === 2) {
    return 'Error Messages'
  } else if (type === 3) {
    return 'Skipped Messages'
  } else {
    return 'Unknown'
  }
}

export function useQueues(queueName: Ref<string>) {
  const query = useQueuesQuery(queueName.value)

  const queueOptions = computed(() => {
    if (!query.data.value) {
      return []
    }

    return [...query.data.value]
      .sort((a, b) => a.type - b.type)
      .map((queue) => ({
        queueNameByType: getQueueName(queue.type),
        queue: queue
      }))
  })

  return {
    query,
    queueOptions
  }
}
