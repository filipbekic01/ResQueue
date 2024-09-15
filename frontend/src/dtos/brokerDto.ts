import type { BrokerAccessDto } from './broker/brokerAccessDto'
import type { BrokerSettingsDto } from './brokerSettingsDto'
import type { RabbitMQConnectionDto } from './RabbitMQConnectionDto'

export interface BrokerDto {
  id: string
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
