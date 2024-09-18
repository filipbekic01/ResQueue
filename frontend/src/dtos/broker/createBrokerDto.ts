import type { CreateRabbitMQConnectionDto } from './createRabbitMQConnectionDto'

export interface CreateBrokerDto {
  name: string
  rabbitMQConnection?: CreateRabbitMQConnectionDto
}
