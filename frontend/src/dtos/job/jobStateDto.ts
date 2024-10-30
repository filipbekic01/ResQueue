export interface JobStateDto {
  jobId: string
  submitted: string
  started: string
  completed: string
  duration: string
  faulted?: boolean
  reason?: string
  lastRetryAttempt: number
  currentState: string
  progressValue?: number
  progressLimit?: number
  jobState?: string
  nextStartDate: string
  isRecurring: boolean
  startDate?: string
  endDate?: string
}
