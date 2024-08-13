import { computed, toValue, type MaybeRef } from 'vue'
import type { MessageDto } from '@/dtos/messageDto'
import type { RabbitMqMessageDto } from '@/dtos/rabbitMqMessageDto'

export function useRabbitMqMessage(dto: MaybeRef<MessageDto>) {
  const message = computed(() => {
    const parsedMessage: RabbitMqMessageDto = {
      ...toValue(dto),
      parsed: JSON.parse(toValue(dto).rawData)
    }

    parsedMessage.parsed.payload = parsedMessage.parsed.payload
      .replace(/\\r\\n/g, '\n')
      .replace(/\\n/g, '\n')

    return parsedMessage
  })

  return {
    message
  }
}
