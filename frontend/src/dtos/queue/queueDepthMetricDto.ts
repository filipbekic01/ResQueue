export interface QueueDepthMetricDto {
  capturedAt: string;
  queueId: number;
  readyCount: number;
  errorCount: number;
  deadLetterCount: number;
}

export interface QueueDepthHistoryDto {
  enabled: boolean;
  sampleIntervalSeconds: number;
  items: QueueDepthMetricDto[];
}
