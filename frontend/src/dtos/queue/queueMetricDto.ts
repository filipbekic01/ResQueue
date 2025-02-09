export interface QueueMetricDto {
  queueMetricId: number
  dateTime: string
  timeSpan: string
  queueId: number
  consumeCount: number
  errorCount: number
  deadLetterCoun: number
}
