import type { MessageDto } from './messageDto'

export interface RabbitMqMessageDto extends MessageDto {
  parsed: {
    payload_bytes: number
    redelivered: boolean
    exchange: string
    routing_key: string
    message_count: number
    properties: {
      delivery_mode: number
      headers: {
        [key: string]: string
      }
    }
    payload: string
    payload_encoding: string
  }
}
