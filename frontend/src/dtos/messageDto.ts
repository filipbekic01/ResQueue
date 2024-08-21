import type { RabbitmqMessageMetadataDto } from './rabbitmqMessageMetadataDto'

export interface MessageDto {
  id: string
  body: any
  bodyEncoding: 'json' | 'base64'
  rabbitmqMetadata?: RabbitmqMessageMetadataDto
  summary: string
  isReviewed: boolean
  createdAt: string
  updatedAt: string
  deletedAt: string
}
