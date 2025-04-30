using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    private readonly ChatMessageContext _chatMessageContext;

    public ChatHub(ChatMessageContext chatMessageContext)
    {
        _chatMessageContext = chatMessageContext;
    }

    public async Task SendMessage(string user, string message)
    {
        var chatMessage = new ChatMessage
        {
            User = user,
            Text = message,
            Timestamp = DateTime.UtcNow
        };

        _chatMessageContext.ChatMessages.Add(chatMessage);

        await _chatMessageContext.SaveChangesAsync();

        await Clients.All.SendAsync("ReceiveMessage", user, message);

        await Clients.All.SendAsync("NewMessageNotification", user, message);
    }

    public override async Task OnConnectedAsync()
    {
        var recentMessages = _chatMessageContext.ChatMessages
        .OrderBy(m => m.Timestamp)
        .Take(50)
        .Select(m => new { m.User, m.Text })
        .ToList();

        await Clients.Caller.SendAsync("LoadHistory", recentMessages);

        await base.OnConnectedAsync();
    }
}
