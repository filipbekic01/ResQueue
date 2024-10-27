export interface RequeueMessagesDto {
  queueName: string
  sourceQueueType: number
  targetQueueType: number
  messageCount: number
  redeliveryCount: number
  delay: number
}
