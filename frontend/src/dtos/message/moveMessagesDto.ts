export interface MoveMessagesDto {
  messages: MoveMessageDeliveryDto[]
  queueName: string
  queueType: number
}

interface MoveMessageDeliveryDto {
  messageDeliveryId: number
  lockId: string
  headers: string
}
