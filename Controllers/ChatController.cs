using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatMessageContext _chatMessageContext;
    public ChatController(ChatMessageContext chatMessageContext) => _chatMessageContext = chatMessageContext;

    [HttpGet("messages")]
    public IActionResult GetMessages()
    {
        var messages = _chatMessageContext.ChatMessages
            .OrderBy(m => m.Timestamp)
            .Take(100)
            .Select(m => new { m.User, m.Text })
            .ToList();

        return Ok(messages);
    }
}

