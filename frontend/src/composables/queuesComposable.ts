import { useQueuesQuery } from '@/api/queues/queuesQuery'
import { computed, type MaybeRef } from 'vue'

export function useQueues(brokerId: MaybeRef<string>) {
  const query = useQueuesQuery(brokerId)

  const formattedQueues = computed(
    () =>
      query.data.value?.map((q) => {
        return {
          ...q,
          parsed: JSON.parse(q.rawData)
        }
      }) ?? []
  )

  return {
    query,
    formattedQueues
  }
}
