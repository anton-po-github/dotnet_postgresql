using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using RestSharp;

public class ZohoAuthService
{
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private readonly RestClient _client;
    private const string TokenCacheKey = "ZohoAccessToken";

    public ZohoAuthService(IConfiguration config, IMemoryCache cache)
    {
        _config = config;
        _cache = cache;
        _client = new RestClient(_config["Zoho:BaseUrl"]);
    }

    public async Task<string> GetAccessTokenAsync(string code)
    {
        var request = new RestRequest("token", Method.Post)
            .AddParameter("grant_type", "authorization_code")
            .AddParameter("client_id", _config["Zoho:ClientId"])
            .AddParameter("client_secret", _config["Zoho:ClientSecret"])
            .AddParameter("redirect_uri", _config["Zoho:RedirectUri"])
            .AddParameter("code", code);

        var response = await _client.ExecuteAsync<ZohoTokenResponse>(request);
        if (response.Data == null)
            throw new InvalidOperationException("Не удалось получить токен от Zoho.");

        var token = response.Data.AccessToken;
        var expiresIn = response.Data.ExpiresIn; // в секундах

        // Гарантируем, что переданное в кэш время положительное:
        // буфер в 60 сек., но не менее 5 минут (300 сек.)
        var bufferTime = Math.Max(expiresIn - 60, 300);
        _cache.Set(TokenCacheKey, token, TimeSpan.FromSeconds(bufferTime));

        return token;
    }

    public Task<string> GetSavedAccessTokenAsync()
    {
        if (_cache.TryGetValue(TokenCacheKey, out string token))
        {
            return Task.FromResult(token);
        }
        throw new InvalidOperationException("Zoho access token is not available. Please authorize first.");
    }
}

public class ZohoTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    [JsonPropertyName("expires_in_sec")]
    public int ExpiresIn { get; set; }
}

