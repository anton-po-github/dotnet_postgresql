using RestSharp;

public class TicketCreateDto
{
    public string Subject { get; set; }
    public string DepartmentId { get; set; }
    public string ContactId { get; set; }
    public string Description { get; set; }
}

public class ZohoListResponse<T>
{
    public List<T> Data { get; set; }
}

public class TicketResponse
{
    public Ticket Data { get; set; }
}

public class Ticket
{
    public string Id { get; set; }
    public string Subject { get; set; }
    public string Status { get; set; }
    public DateTime CreatedTime { get; set; }
}

public class ZohoTicketService
{
    private readonly RestClient _client;
    private readonly ZohoAuthService _auth;

    public ZohoTicketService(IConfiguration config, ZohoAuthService auth)
    {
        _client = new RestClient(config["Zoho:BaseUrl"]);
        _auth = auth;
    }

    private async Task<RestRequest> CreateRequestAsync(string resource, Method method)
    {
        // Предполагаем, что токен хранится или передается
        var token = await _auth.GetSavedAccessTokenAsync();
        var req = new RestRequest(resource, method)
            .AddHeader("Authorization", $"Zoho-oauthtoken {token}")
            .AddHeader("Content-Type", "application/json");
        return req;
    }

    public async Task<Ticket> CreateTicketAsync(TicketCreateDto dto)
    {
        var req = await CreateRequestAsync("tickets", Method.Post);

        req.AddJsonBody(dto);

        var resp = await _client.ExecuteAsync<TicketResponse>(req);
        return resp.Data.Data;
    }

    public async Task<List<Ticket>> GetTicketsAsync()
    {
        var req = await CreateRequestAsync("tickets", Method.Get);

        var resp = await _client.ExecuteAsync<ZohoListResponse<Ticket>>(req);

        return resp.Data.Data;
    }
}
