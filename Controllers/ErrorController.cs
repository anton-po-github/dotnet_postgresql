using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("errors")]
public class ErrorController : ControllerBase
{
    [HttpGet("{code:int}")]
    public IActionResult HandleError(int code)
    {
        var detail = code switch
        {
            401 => new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized",
                Detail = "Authentication is required to access this resource."
            },
            403 => new ProblemDetails
            {
                Status = 403,
                Title = "Forbidden",
                Detail = "You do not have permission to access this resource."
            },
            _ => new ProblemDetails
            {
                Status = code,
                Title = "Error",
                Detail = "An error occurred."
            }
        };
        return StatusCode(code, detail);
    }
}
