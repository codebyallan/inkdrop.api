namespace Inkdrop.Api.Core;

public class ServiceResult<T>
{
    public T? Value { get; }
    public bool IsSuccess { get; }
    public bool IsNotFound { get; }

    private ServiceResult(T? value, bool isSuccess, bool isNotFound)
    {
        Value = value;
        IsSuccess = isSuccess;
        IsNotFound = isNotFound;
    }

    public static ServiceResult<T> Success(T? value) => new(value, true, false);
    public static ServiceResult<T> NotFound() => new(default, false, true);
    public static ServiceResult<T> Failure() => new(default, false, false);
}
