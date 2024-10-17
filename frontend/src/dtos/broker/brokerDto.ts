import type { BrokerAccessDto } from './brokerAccessDto'
import type { PostgresConnectionDto } from './postgresConnectionDto'
import type { RabbitMQConnectionDto } from './rabbitMQConnectionDto'

export interface BrokerDto {
  id: string
  userId: string
  createdByUserId: string
  accessList: BrokerAccessDto[]
  system: string
  name: string
  rabbitMQConnection: RabbitMQConnectionDto
  postgresConnection: PostgresConnectionDto
  createdAt: string
  updatedAt: string
  syncedAt: string
  deletedAt: string
}
