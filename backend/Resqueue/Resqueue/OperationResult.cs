using Microsoft.AspNetCore.Mvc;

namespace Resqueue;

public class OperationResult<T>
{
    public T? Value { get; private set; }
    public ProblemDetails? Problem { get; private set; }
    public bool IsSuccess => Problem == null;

    private OperationResult(T value)
    {
        Value = value;
    }

    private OperationResult(ProblemDetails problem)
    {
        Problem = problem;
    }

    public static OperationResult<T> Success(T value) => new OperationResult<T>(value);
    public static OperationResult<T> Failure(ProblemDetails problem) => new OperationResult<T>(problem);
}