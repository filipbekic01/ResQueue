import type { MessageDto } from '@/dtos/message/messageDto'

export function messageSummary(message: MessageDto) {
  if (
    !message.rabbitmqMetadata ||
    !message.rabbitmqMetadata.properties ||
    !message.rabbitmqMetadata.properties.headers
  ) {
    return ''
  }

  const headers = message.rabbitmqMetadata.properties.headers

  let lastHope = 'Summary not available.'

  for (const key in headers) {
    const readyKey = key.trim().toLowerCase()

    if (readyKey.includes('mt-fault-message')) {
      return headers[key]
    } else if (readyKey.includes('nservicebus.exceptioninfo.message')) {
      return headers[key]
    } else if (readyKey.includes('x-death')) {
      return headers[key]
    }

    if (
      (readyKey.includes('error') ||
        readyKey.includes('fault') ||
        readyKey.includes('fail') ||
        readyKey.includes('exception')) &&
      !readyKey.includes('trace')
    ) {
      lastHope = String(headers[key])
    }
  }

  return lastHope
}
