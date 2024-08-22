import type { RabbitmqMessagePropertiesDto } from './rabbitmqMessagePropertiesDto'

export interface RabbitmqMessageMetadataDto {
  redelivered: boolean
  exchange: string
  routingKey: string
  properties: RabbitmqMessagePropertiesDto
}
