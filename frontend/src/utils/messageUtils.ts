import type { MessageDto } from '@/dtos/messageDto'

export function messageSummary(message: MessageDto) {
  if (
    !message.rabbitmqMetadata ||
    !message.rabbitmqMetadata.properties ||
    !message.rabbitmqMetadata.properties.headers
  ) {
    return ''
  }

  const headers = message.rabbitmqMetadata.properties.headers

  for (const key in headers) {
    const readyKey = key.trim().toLowerCase()
    console.log(readyKey)

    if (readyKey.includes('mt-fault-message')) {
      return 'mt-fault-message'
    } else if (readyKey.includes('nservicebus.exceptioninfo.message')) {
      return 'nservicebus.exceptioninfo.message'
    } else if (readyKey.includes('x-death')) {
      return 'x-death'
    } else if (
      readyKey.includes('error') ||
      readyKey.includes('fault') ||
      readyKey.includes('fail') ||
      readyKey.includes('exception')
    ) {
      return 'errorfaultfailexception'
    }
  }

  return ''
}
