export interface QueueViewDto {
  queueName: string
  queueAutoDelete?: number
  ready: number
  scheduled: number
  errored: number
  deadLettered: number
  locked: number
  consumeCount: number
  errorCount: number
  deadLetterCount: number
  countStartTime?: string
  countDuration: number
  queueMaxDeliveryCount: number
}
