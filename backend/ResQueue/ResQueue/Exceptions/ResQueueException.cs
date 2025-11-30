namespace ResQueue.Exceptions;

public class ResQueueException : Exception
{
    public int StatusCode { get; }
    public string? Detail { get; }

    public ResQueueException(string message, int statusCode = 400, string? detail = null)
        : base(message)
    {
        StatusCode = statusCode;
        Detail = detail;
    }

    public ResQueueException(string message, int statusCode, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
