import type { QueueDto } from './queueDto'

export interface RabbitMqQueueDto extends QueueDto {
  parsed: {
    messages: number
    name: string
  }
}
