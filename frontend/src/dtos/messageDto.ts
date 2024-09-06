import type { RabbitMQMessageMetadataDto } from './rabbitMQMessageMetaDto'

export interface MessageDto {
  id: string
  body: any
  bodyEncoding: 'json' | 'base64'
  rabbitmqMetadata?: RabbitMQMessageMetadataDto
  summary: string
  isReviewed: boolean
  createdAt: string
  updatedAt: string
  deletedAt: string
}
