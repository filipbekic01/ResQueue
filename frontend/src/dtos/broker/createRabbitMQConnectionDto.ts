export interface CreateRabbitMQConnectionDto {
  username: string
  password: string
  managementPort: number
  managementTls: boolean
  amqpPort: number
  amqpTls: boolean
  host: string
  vHost: string
}
