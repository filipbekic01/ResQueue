import type { ProblemDetails } from "@/dtos/problemDetails";

export interface ToastError {
  title: string;
  detail: string;
}

const GENERIC_ERROR: ProblemDetails = {
  title: "Server Error",
  detail: "An unexpected error occurred while communicating with the server.",
};

export const errorToProblemDetails = (error: any): ProblemDetails => {
  const hasOwnProp = Object.prototype.hasOwnProperty;

  // Check if the error response exists
  if (error.response) {
    const contentType = error.response.headers?.["content-type"] || "";

    // Only parse as ProblemDetails if content type is application/problem+json
    if (contentType.includes("application/problem+json") && error.response.data) {
      const problemDetails = error.response.data as ProblemDetails;
      const result: ProblemDetails = {
        title: problemDetails.title || GENERIC_ERROR.title,
        detail: problemDetails.detail || GENERIC_ERROR.detail,
      };

      // Check if there are errors in the response
      if (problemDetails.errors && typeof problemDetails.errors === "object") {
        const errorMessages: string[] = [];

        // Iterate through the errors dictionary and extract messages
        for (const key in problemDetails.errors) {
          if (hasOwnProp.call(problemDetails.errors, key)) {
            const messages = problemDetails.errors[key];
            if (Array.isArray(messages)) {
              errorMessages.push(...messages);
            }
          }
        }

        // Assign error messages if found
        if (errorMessages.length > 0) {
          result.errors = errorMessages.reduce(
            (acc, message) => {
              acc.general = acc.general || [];
              acc.general.push(message);
              return acc;
            },
            {} as { [key: string]: string[] },
          );
        }
      }

      return result;
    }

    // Non-problem+json response - return generic error
    return { ...GENERIC_ERROR };
  } else if (error.message) {
    // Network error or other error with a message
    return {
      title: "Error",
      detail: error.message,
    };
  }

  // Fallback to the generic error
  return { ...GENERIC_ERROR };
};

export const errorToToast = (error: any): ToastError => {
  const problemDetails = errorToProblemDetails(error);

  return {
    title: problemDetails.title || "Error",
    detail: problemDetails.detail || "An unexpected error occurred.",
  };
};
