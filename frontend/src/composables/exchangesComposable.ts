import { useExchangesQuery } from '@/api/exchanges/exchangesQuery'
import { computed, type MaybeRef } from 'vue'

export function useExchanges(brokerId: MaybeRef<string>) {
  const query = useExchangesQuery(brokerId)

  const formattedExchanges = computed(
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
    formattedExchanges
  }
}
