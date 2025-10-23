using System.Net.Http.Headers;
using System.Net.Http.Json; // ← ここが必須
using System.Text.Json.Serialization;

class Program
{
    static async Task Main()
    {
        // Keycloak情報
        var tokenUrl = "http://localhost:8080/realms/sandbox/protocol/openid-connect/token";
        var clientId = "filelinkage-client";
        var clientSecret = "<ここにSecretを貼り付け>";
        
        // 送信するファイル
        var filePath = "test.txt"; // 任意のファイルパス
        
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File '{filePath}' not found.");
            Console.WriteLine("Creating a test file...");
            await File.WriteAllTextAsync(filePath, "This is a test file created by FileLinkClient.");
        }
        
        var apiUrl = "http://localhost:5000/api/files/upload";

        using var http = new HttpClient();

        // 1️⃣ トークン取得
        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret)
        });

        var tokenResponse = await http.PostAsync(tokenUrl, tokenRequest);
        tokenResponse.EnsureSuccessStatusCode();

        // null 許容型に修正して警告回避
        var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        var accessToken = tokenJson?.access_token ?? throw new Exception("Token not received");

        Console.WriteLine($"Access Token: {accessToken.Substring(0, 20)}...");

        // 2️⃣ ファイル送信
        using var content = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(filePath);
        content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));

        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await http.PostAsync(apiUrl, content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Response from API:");
        Console.WriteLine(result);
    }

    class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? access_token { get; set; } = null;

        [JsonPropertyName("token_type")]
        public string? token_type { get; set; } = null;

        [JsonPropertyName("expires_in")]
        public int expires_in { get; set; }

        [JsonPropertyName("refresh_expires_in")]
        public int refresh_expires_in { get; set; }

        [JsonPropertyName("scope")]
        public string? scope { get; set; } = null;
    }
}
