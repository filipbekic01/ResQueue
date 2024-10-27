export interface MessageDto {
  transportMessageId: string
  contentType: string
  messageType: string
  body: string
  binaryBody: Uint8Array
  messageId: string
  correlationId: string
  conversationId: string
  requestId: string
  initiatorId: string
  schedulingTokenId: string
  sourceAddress: string
  destinationAddress: string
  responseAddress: string
  faultAddress: string
  sentTime: string
  headers: any
  host: any
}
