import type { CreateRabbitMQConnectionDto } from './createRabbitMQConnectionDto'
import type { PostgresConnectionDto } from './postgresConnectionDto'

export interface CreateBrokerDto {
  name: string
  rabbitMQConnection?: CreateRabbitMQConnectionDto
  postgresConnection?: PostgresConnectionDto
}
