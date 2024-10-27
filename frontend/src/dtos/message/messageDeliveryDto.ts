import type { MessageDto } from './messageDto'

export interface MessageDeliveryDto {
  messageDeliveryId: number
  transportMessageId: string
  queueId: number
  priority: number
  enqueueTime: string
  expirationTime: string
  partitionKey: string
  routingKey: string
  consumerId: string
  lockId: string
  deliveryCount: number
  maxDeliveryCount: number
  lastDelivered?: string
  transportHeaders: any
  message: MessageDto
}
