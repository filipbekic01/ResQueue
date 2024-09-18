import type { MessageDto } from './messageDto'

export interface RabbitMQMessageDto extends MessageDto {
  parsed: {
    payload_bytes: number
    redelivered: boolean
    exchange: string
    routing_key: string
    message_count: number
    properties: {
      message_id: string
      delivery_mode: number
      headers: {
        [key: string]: string
      }
      content_type: string
    }
    payload: string
    payload_encoding: string
  }
}
