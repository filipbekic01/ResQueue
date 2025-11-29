export interface QueueMetricDto {
  queueMetricId: number;
  startTime: string;
  queueId: number;
  consumeCount: number;
  errorCount: number;
  deadLetterCount: number;
}
