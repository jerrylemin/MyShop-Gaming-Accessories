namespace ProjectTest.Models;

public class OperationResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public static OperationResult Ok(string message = "")
    {
        return new OperationResult { Success = true, Message = message };
    }

    public static OperationResult Fail(string message)
    {
        return new OperationResult { Success = false, Message = message };
    }
}
