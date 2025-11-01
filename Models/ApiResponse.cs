namespace MUMbackend.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public object? Errors { get; set; }

        public ApiResponse(bool success, string message, T? data = default, object? errors = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = errors;
        }

        public static ApiResponse<T> Ok(string message, T? data = default)
        {
            return new ApiResponse<T>(true, message, data);
        }

        public static ApiResponse<T> Fail(string message, object? errors = null)
        {
            return new ApiResponse<T>(false, message, default, errors);
        }
    }
}
