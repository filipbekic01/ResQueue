import type { MessageDto } from './messageDto'

export interface MessageDeliveryDto {
  message_delivery_id: number // long in C# maps to number in TS
  transport_message_id: string
  queue_id: number // long in C# maps to number in TS
  priority: number // short in C# maps to number in TS
  enqueue_time: Date
  expiration_time: Date
  partition_key: string
  routing_key: string
  consumer_id: string
  lock_id: string
  delivery_count: number
  max_delivery_count: number
  last_delivered: Date
  transport_headers: string

  message: MessageDto // Message interface reference
}
