export interface RequeueSpecificMessagesDto {
  messageDeliveryIds: number[]
  targetQueueType: number
  redeliveryCount: number
  delay: number
  transactional: boolean
}
