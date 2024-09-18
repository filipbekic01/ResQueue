import type { BrokerSettingsDto } from './brokerSettingsDto'
import type { UpdateRabbitMQConnectionDto } from './updateRabbitMQConnectionDto'

export interface UpdateBrokerDto {
  name: string
  rabbitMQConnection?: UpdateRabbitMQConnectionDto
  settings: BrokerSettingsDto
}
