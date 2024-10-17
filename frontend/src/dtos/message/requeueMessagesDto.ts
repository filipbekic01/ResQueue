export interface RequeueMessagesDto {
  queueName: string
  sourceQueueType: number
  targetQueueType: number
  messageCount: number
  redeliveryCoun: number
}
