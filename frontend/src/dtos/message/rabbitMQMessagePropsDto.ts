export type HeaderValue = string | number | boolean | HeaderValue[]

export interface RabbitMQMessagePropsDto {
  appId?: string
  clusterId?: string
  contentEncoding?: string
  contentType?: string
  correlationId?: string
  deliveryMode?: number
  expiration?: string
  headers?: Record<string, HeaderValue>
  messageId?: string
  priority?: number
  replyTo?: string
  timestamp?: number
  type?: string
  userId?: string
}
