namespace ProjectTest.Models;

public class OperationResult<T> : OperationResult
{
    public T? Value { get; init; }

    public static OperationResult<T> Ok(T value, string message = "")
    {
        return new OperationResult<T> { Success = true, Value = value, Message = message };
    }

    public static new OperationResult<T> Fail(string message)
    {
        return new OperationResult<T> { Success = false, Message = message };
    }
}
