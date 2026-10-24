namespace ReCenterHub.Services
{
    public class ApiAuthService
    {
        private readonly HttpClient _httpClient;

        public ApiAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponse?> LoginAsync(
                    string email,
                    string password,
                    string? authenticatorCode = null)
        {
            var loginData = new Dictionary<string, string>
            {
                ["email"] = email,
                ["password"] = password
            };

            if (!string.IsNullOrWhiteSpace(authenticatorCode))
            {
                var code = authenticatorCode.Trim();

                var stripped = code.Replace(" ", "").Replace("-", "");

                if (code.Contains('-') && stripped.Length > 6)
                {
                    loginData["twoFactorRecoveryCode"] = code.Replace(" ", "");
                }
                else
                {
                    loginData["twoFactorCode"] = stripped;
                }
            }

            using var response = await _httpClient.PostAsJsonAsync(
                "login?useCookies=false",
                loginData);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<LoginResponse>();
        }

        public async Task<CurrentUser?> GetCurrentUserAsync(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/account/me");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CurrentUser>(
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task<bool> RegisterAsync(
            string email,
            string password)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/account/register",
                new
                {
                    email,
                    password
                });

            return response.IsSuccessStatusCode;
        }
    }

    public class CurrentUser
    {
        public string? Id { get; set; }

        public string? Email { get; set; }

        public List<string> Roles { get; set; } = new();
    }

    public class LoginResponse
    {
        public string? TokenType { get; set; }

        public string? AccessToken { get; set; }

        public int ExpiresIn { get; set; }

        public string? RefreshToken { get; set; }
    }
}