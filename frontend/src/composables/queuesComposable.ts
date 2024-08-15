import { useQueuesQuery } from '@/api/queues/queuesQuery'
import { computed, type MaybeRef, type Ref } from 'vue'

export function useQueues(brokerId: MaybeRef<string>, pageIndex: Ref<number>) {
  const query = useQueuesQuery(brokerId, pageIndex)

  const formattedQueues = computed(
    () =>
      query.data.value?.items.map((q) => {
        const item = {
          ...q,
          parsed: JSON.parse(q.rawData)
        }

        return item
      }) ?? []
  )

  return {
    query,
    formattedQueues
  }
}
