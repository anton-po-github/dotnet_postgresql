using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ZohoTicketsController : ControllerBase
{
    private readonly ZohoAuthService _auth;
    private readonly ZohoTicketService _service;

    public ZohoTicketsController(ZohoAuthService auth, ZohoTicketService service)
    {
        _auth = auth;
        _service = service;
    }

    // 1) GET-endpoint для получения кода от Zoho
    [HttpGet("signin-zoho")]
    public async Task<IActionResult> SigninZoho([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Code is missing");
        // здесь вы обменяете code на токен и сохраните
        await _auth.GetAccessTokenAsync(code);
        // перенаправляете пользователя назад в UI
        return Redirect("http://localhost:4200");
    }

    // 2) Создание тикета (POST api/ZohoTickets)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TicketCreateDto dto)
    {
        var ticket = await _service.CreateTicketAsync(dto);
        return Ok(ticket);
    }

    // 3) Список тикетов (GET api/ZohoTickets)
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var tickets = await _service.GetTicketsAsync();
        return Ok(tickets);
    }
}
