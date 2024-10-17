export interface MessageDto {
  transport_message_id: string
  content_type: string
  message_type: string
  body: string
  binary_body: Uint8Array
  message_id: string
  correlation_id: string
  conversation_id: string
  request_id: string
  initiator_id: string
  scheduling_token_id: string
  source_address: string
  destination_address: string
  response_address: string
  fault_address: string
  sent_time: Date
  headers: string
  host: string
}
