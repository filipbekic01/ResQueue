import type { RabbitMQUpsertMessageMetaDto } from './rabbitMQUpsertMessageMetaDto'

export interface UpsertMessageDto {
  brokerId: string
  queueId: string
  body: any
  bodyEncoding: 'json' | 'base64' | 'string'
  rabbitmqMetadata?: RabbitMQUpsertMessageMetaDto
}
