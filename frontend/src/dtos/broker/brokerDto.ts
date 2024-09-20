import type { BrokerAccessDto } from './brokerAccessDto'
import type { RabbitMQConnectionDto } from './rabbitMQConnectionDto'

export interface BrokerDto {
  id: string
  userId: string
  accessList: BrokerAccessDto[]
  system: string
  name: string
  rabbitMQConnection: RabbitMQConnectionDto
  createdAt: string
  updatedAt: string
  syncedAt: string
  deletedAt: string
}
