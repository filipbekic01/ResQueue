import type { BrokerSettingsDto } from './brokerSettingsDto'
import type { PostgresConnectionDto } from './postgresConnectionDto'
import type { UpdateRabbitMQConnectionDto } from './updateRabbitMQConnectionDto'

export interface UpdateBrokerDto {
  name: string
  rabbitMQConnection?: UpdateRabbitMQConnectionDto
  postgresConnection?: PostgresConnectionDto
  settings: BrokerSettingsDto
}
