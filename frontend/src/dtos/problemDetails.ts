export interface ProblemDetails {
  type?: string // A URI reference that identifies the problem type
  title?: string // A short, human-readable summary of the problem
  status?: number // The HTTP status code
  detail?: string // A human-readable explanation specific to this occurrence of the problem
  instance?: string // A URI reference that identifies the specific occurrence of the problem
  errors?: {
    // Optional object to hold validation or other errors
    [key: string]: string[] // Dictionary of error messages with the key being the field and the value being an array of messages
  }
}
