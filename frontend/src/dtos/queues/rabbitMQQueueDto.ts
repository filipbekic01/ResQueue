import type { QueueDto } from './queueDto'

export interface RabbitMQQueueDto extends QueueDto {
  parsed: {
    messages: number
    name: string
  }
}
