import type { RabbitMQMessagePropsDto } from './rabbitMQMessagePropsDto'

export interface RabbitMQMessageMetaDto {
  redelivered: boolean
  exchange: string
  routingKey: string
  properties: RabbitMQMessagePropsDto
}
