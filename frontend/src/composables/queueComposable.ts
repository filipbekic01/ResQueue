import { useQueuesQuery } from '@/api/queues/queuesQuery'
import { useQueueViewQuery } from '@/api/queues/queueViewQuery'
import type { QueueViewDto } from '@/dtos/queue/queueViewDto'
import { computed, toValue, type Ref } from 'vue'
import { useUserSettings } from './userSettingsComposable'

const getQueueName = (type: number, queueView?: QueueViewDto) => {
  if (type === 1) {
    return `Ready (${queueView?.ready})`
  } else if (type === 2) {
    return `Error (${queueView?.errored})`
  } else if (type === 3) {
    return `Dead-Letter (${queueView?.deadLettered})`
  } else {
    return 'Unknown'
  }
}

const getQueueTypeLabel = (type?: number) => {
  if (type === 1) {
    return `ready`
  } else if (type === 2) {
    return `error`
  } else if (type === 3) {
    return `dead-letter`
  } else {
    return 'Unknown'
  }
}

export function useQueue(queueName: Ref<string>) {
  const { settings } = useUserSettings()

  const query = useQueuesQuery(queueName, settings.refetchInterval)
  const queryView = useQueueViewQuery(queueName, settings.refetchInterval)

  const queueOptions = computed(() => {
    if (!query.data.value) {
      return []
    }

    return [...query.data.value]
      .sort((a, b) => a.type - b.type)
      .map((queue) => ({
        queueNameByType: getQueueName(queue.type, toValue(queryView.data)),
        queue: queue
      }))
  })

  return {
    query,
    queryView,
    queueOptions,
    getQueueTypeLabel
  }
}
