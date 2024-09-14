import type { RabbitMQMessagePropsDto } from './rabbitMQMessagePropsDto'

export interface RabbitMQUpsertMessageMetaDto {
  routingKey: string
  properties: RabbitMQMessagePropsDto
}
