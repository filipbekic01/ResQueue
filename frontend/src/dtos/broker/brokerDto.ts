import type { BrokerAccessDto } from './brokerAccessDto'
import type { BrokerSettingsDto } from './brokerSettingsDto'
import type { RabbitMQConnectionDto } from './rabbitMQConnectionDto'

export interface BrokerDto {
  id: string
  userId: string
  accessList: BrokerAccessDto[]
  system: string
  name: string
  rabbitMQConnection: RabbitMQConnectionDto
  settings: BrokerSettingsDto
  createdAt: string
  updatedAt: string
  syncedAt: string
  deletedAt: string
}
