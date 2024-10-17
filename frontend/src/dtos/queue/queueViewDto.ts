export interface QueueViewDto {
  queueName: string
  queueAutoDelete: boolean
  ready: number
  scheduled: number
  errored: number
  deadLettered: number
  locked: number
  consumeCount: number
  errorCount: number
  deadLetterCount: number
  countDuration: number
}
