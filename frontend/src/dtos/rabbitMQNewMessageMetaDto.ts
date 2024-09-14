import type { RabbitMQMessagePropsDto } from './rabbitMQMessagePropsDto'

export interface RabbitMQNewMessageMetaDto {
  routingKey: string
  properties: RabbitMQMessagePropsDto
}
