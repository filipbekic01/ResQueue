export interface DeleteMessagesDto {
  messages: Message[]
}

interface Message {
  messageDeliveryId: number
  lockId: string
}
