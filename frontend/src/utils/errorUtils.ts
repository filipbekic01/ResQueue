const extractErrorMessage = (error: any) => {
  const hasOwnProp = Object.prototype.hasOwnProperty

  // Check if the error response exists and has data
  if (error.response?.data) {
    const problemDetails = error.response.data

    // Check if there are errors in the response
    if (problemDetails.errors && typeof problemDetails.errors === 'object') {
      const errorMessages = []

      // Iterate through the errors dictionary and extract messages
      for (const key in problemDetails.errors) {
        if (hasOwnProp.call(problemDetails.errors, key)) {
          const messages = problemDetails.errors[key]
          if (Array.isArray(messages)) {
            errorMessages.push(...messages)
          }
        }
      }

      // Join all error messages into a single string
      if (errorMessages.length > 0) {
        return errorMessages.join(' ')
      }
    }

    // Fallback to the title or detail from ProblemDetails
    if (problemDetails.title) {
      return problemDetails.title
    } else if (problemDetails.detail) {
      return problemDetails.detail
    }
  }

  // Fallback to a generic error message
  return error.response?.data ?? 'An unexpected error occurred.'
}

export { extractErrorMessage }
