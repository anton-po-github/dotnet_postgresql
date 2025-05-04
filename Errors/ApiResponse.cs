public class ApiResponse
{
    public ApiResponse(int statusCode, string message = "")
    {
        StatusCode = statusCode;
        // If message is empty or null, take the default message
        Message = string.IsNullOrEmpty(message)
            ? GetDefaultMessageForStatusCode(statusCode)
            : message;
    }

    public int StatusCode { get; set; }
    public string? Message { get; set; }

    private string GetDefaultMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 => "A bad request, you have made",
            401 => "Authorized, you are not",
            404 => "Resource found, it was not",
            500 => "Internal Server Error",
            _ => "Empty response"
        };
    }
}

public class ApiValidationErrorResponse : ApiResponse
{
    // 1) Initialize Errors immediately upon declaration
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

    public ApiValidationErrorResponse()
        : base(400)
    {
        // 2) (Optional) You can add any standard error messages here
        // Errors = new List<string> { "Validation failed." };
    }
}


