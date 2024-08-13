import { useMessagesQuery } from '@/api/messages/messagesQuery'
import { computed, type MaybeRef } from 'vue'

export function useMessages(queueId: MaybeRef<string>) {
  const query = useMessagesQuery(queueId)

  const formattedMessages = computed(
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
    formattedMessages
  }
}
