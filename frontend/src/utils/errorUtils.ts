import type { ProblemDetails } from '@/dtos/problemDetails'
import type { ToastMessageOptions } from 'primevue/toast'

export const errorToProblemDetails = (error: any): ProblemDetails => {
  const hasOwnProp = Object.prototype.hasOwnProperty

  // Create a default ProblemDetails structure
  const defaultProblemDetails: ProblemDetails = {
    title: 'Error',
    detail: 'An unexpected error occurred.'
  }

  // Check if the error response exists and has data
  if (error.response?.data) {
    const problemDetails = error.response.data as ProblemDetails
    const result: ProblemDetails = { ...defaultProblemDetails }

    // Check if there are errors in the response
    if (problemDetails.errors && typeof problemDetails.errors === 'object') {
      const errorMessages: string[] = []

      // Iterate through the errors dictionary and extract messages
      for (const key in problemDetails.errors) {
        if (hasOwnProp.call(problemDetails.errors, key)) {
          const messages = problemDetails.errors[key]
          if (Array.isArray(messages)) {
            errorMessages.push(...messages)
          }
        }
      }

      // Assign error messages if found
      if (errorMessages.length > 0) {
        result.errors = errorMessages.reduce(
          (acc, message) => {
            acc.general = acc.general || []
            acc.general.push(message)
            return acc
          },
          {} as { [key: string]: string[] }
        )
      }
    }

    // Set title and detail if available
    if (problemDetails.title) {
      result.title = problemDetails.title
    }
    if (problemDetails.detail) {
      result.detail = problemDetails.detail
    }

    return result
  }

  // Fallback to the default problem details if no response
  return defaultProblemDetails
}

export const errorToToast = (error: any): ToastMessageOptions => {
  const problemDetails = errorToProblemDetails(error)

  return {
    severity: 'error',
    summary: problemDetails.title || 'Error',
    detail: problemDetails.detail || 'An unexpected error occurred.',
    life: 3000
  }
}
