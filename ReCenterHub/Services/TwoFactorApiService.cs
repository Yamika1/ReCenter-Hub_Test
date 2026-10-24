using System.Net.Http.Headers;

namespace ReCenterHub.Services
{
    public class TwoFactorApiService
    {
        private readonly HttpClient _httpClient;

        public TwoFactorApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TwoFactorApiResponse> BeginSetupAsync(
            string accessToken)
        {
            return await SendAsync(
                accessToken,
                new { });
        }

        public async Task<TwoFactorApiResponse> EnableAsync(
            string accessToken,
            string code)
        {
            return await SendAsync(accessToken, new
            {
                enable = true,
                twoFactorCode = code.Replace(" ", "").Replace("-", "")
            });
        }

        private async Task<TwoFactorApiResponse> SendAsync(
            string accessToken,
            object body)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "manage/2fa");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Content = JsonContent.Create(body);

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<TwoFactorApiResponse>()
                ?? new TwoFactorApiResponse();
        }
    }

    public class TwoFactorApiResponse
    {
        public string? SharedKey { get; set; }

        public bool IsTwoFactorEnabled { get; set; }

        public int RecoveryCodesLeft { get; set; }

        public List<string>? RecoveryCodes { get; set; }
    }
}