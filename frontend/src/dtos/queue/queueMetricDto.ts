export interface QueueMetricDto {
  queueMetricId: number
  startTime: string
  timeSpan: string
  queueId: number
  consumeCount: number
  errorCount: number
  deadLetterCount: number
}
