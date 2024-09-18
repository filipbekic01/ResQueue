import type { RabbitMQMessagePropsDto } from './rabbitMQMessagePropsDto'

export interface RabbitMQMessageMetaDto {
  redelivered: boolean
  exchange: string | null
  routingKey: string
  properties: RabbitMQMessagePropsDto
}
