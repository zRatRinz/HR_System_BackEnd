namespace HR_System.Api.Api.Common;

public class ApiResponse<T>
{
    public string Status { get; set; } = "success";
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string message = "")
    {
        return new ApiResponse<T>
        {
            Status = "success",
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T>
        {
            Status = "fail",
            Message = message,
            Data = default
        };
    }

    //public static ApiResponse<TData> Success<TData>(TData data, string message = "")
    //{
    //    return new ApiResponse<TData>
    //    {
    //        Status = "success",
    //        Message = message,
    //        Data = data
    //    };
    //}

    //public static ApiResponse<TData> Fail<TData>(string message)
    //{
    //    return new ApiResponse<TData>
    //    {
    //        Status = "fail",
    //        Message = message,
    //        Data = default
    //    };
    //}
}

public class ApiResponse
{
    public string Status { get; set; } = "success";
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    public static ApiResponse<T> Success<T>(T data, string message = "")
    {
        return new ApiResponse<T>
        {
            Status = "success",
            Message = message,
            Data = data
        };
    }

    public static ApiResponse Success(object data, string message = "")
    {
        return new ApiResponse
        {
            Status = "success",
            Message = message,
            Data = data
        };
    }

    public static ApiResponse Fail(string message)
    {
        return new ApiResponse
        {
            Status = "fail",
            Message = message,
            Data = null
        };
    }
}
