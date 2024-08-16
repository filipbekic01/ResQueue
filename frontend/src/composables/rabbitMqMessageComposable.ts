import { computed, toValue, type MaybeRef } from 'vue'
import type { MessageDto } from '@/dtos/messageDto'
import type { RabbitMqMessageDto } from '@/dtos/rabbitMqMessageDto'

export function useRabbitMqMessage(message: MaybeRef<MessageDto | undefined>) {
  const rabbitMqMessage = computed(() => {
    const value = toValue(message)

    if (!value) {
      return undefined
    }

    const parsedMessage: RabbitMqMessageDto = {
      ...toValue(value),
      parsed: JSON.parse(value.rawData)
    }

    parsedMessage.parsed.payload = parsedMessage.parsed.payload
      .replace(/\\r\\n/g, '\n')
      .replace(/\\n/g, '\n')

    return parsedMessage
  })

  return {
    rabbitMqMessage
  }
}
