namespace TimeTraceOne.DTOs;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    
    public static ApiResponse<T> Success(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }
    
    public static ApiResponse<T> Error(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors
        };
    }
}

public class PaginatedResponse<T> : ApiResponse<List<T>>
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
    
    public static PaginatedResponse<T> CreateSuccess(List<T> data, int page, int limit, int total, int totalPages)
    {
        return new PaginatedResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Page = page,
            Limit = limit,
            Total = total,
            TotalPages = totalPages
        };
    }
}
