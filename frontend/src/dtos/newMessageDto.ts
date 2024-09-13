import type { RabbitMQNewMessageMetaDto } from './rabbitMQNewMessageMetaDto'

export interface NewMessageDto {
  brokerId: string
  body: any
  bodyEncoding: 'json' | 'base64'
  rabbitmqMetadata?: RabbitMQNewMessageMetaDto
}
