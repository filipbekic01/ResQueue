import type { RabbitMQNewMessageMetaDto } from './rabbitMQNewMessageMetaDto'

export interface CreateMessageDto {
  brokerId: string
  queueId: string
  body: any
  bodyEncoding: 'json' | 'base64'
  rabbitmqMetadata?: RabbitMQNewMessageMetaDto
}
