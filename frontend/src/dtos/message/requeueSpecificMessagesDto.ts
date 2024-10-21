export interface RequeueSpecificMessagesDto {
  messageDeliveryIds: number[]
  targetQueueType: number
  redeliveryCount: number
  delay: string
}
