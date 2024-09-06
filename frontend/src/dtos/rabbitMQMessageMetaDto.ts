import type { RabbitMQMessagePropsDto } from './rabbitMQMessagePropsDto'

export interface RabbitMQMessageMetadataDto {
  redelivered: boolean
  exchange: string
  routingKey: string
  properties: RabbitMQMessagePropsDto
}
