namespace BookStorage.Core.Entities;

public class Result<T>
{
    public bool IsSuccess => ResultCode == 0;
    public ResultCodes ResultCode { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(ResultCodes resultCode, T? value, string? error)
    {
        ResultCode = resultCode;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(0, value, null);
    public static Result<T> Failure(ResultCodes resultCode, string error) => new Result<T>(resultCode, default, error);
}

public enum ResultCodes
{
    Success = 0,
    #region StandardErrors
    InnerError = 1,
    NotFound = 4,
    InvalidInput = 5,
    #endregion
    //Books(200-299)
    //Categories (300-399)
    #region Categories (300-399)
    CategoryRemovingNoAllowedBySubCategories = 300,
    CategorySameName = 301,
    #endregion
    //BookFiles(400-499)
    //Persons(500-599)
}
