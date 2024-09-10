import type { RabbitMQMessageMetaDto } from './rabbitMQMessageMetaDto'

export interface MessageDto {
  id: string
  body: any
  bodyEncoding: 'json' | 'base64'
  rabbitmqMetadata?: RabbitMQMessageMetaDto
  isReviewed: boolean
  createdAt: string
  updatedAt: string
  deletedAt: string
}
