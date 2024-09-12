import type { RabbitMQMessagePropsDto } from './rabbitMQMessagePropsDto'

export interface RabbitMQNewMessageMetaDto {
  exchange: string
  routingKey: string
  properties: RabbitMQMessagePropsDto
}
