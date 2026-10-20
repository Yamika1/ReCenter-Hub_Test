using ReCenterHub.Models;
using System.Net;
using System.Net.Http.Headers;

namespace ReCenterHub.Services
{
    public class NoticeService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NoticeService(HttpClient httpClient,IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
           _httpContextAccessor = httpContextAccessor;
        }

        private void AddToken()
        {
            var token = _httpContextAccessor.HttpContext?
                .Session.GetString("AccessToken");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }


        private static void ThrowIfUnauthorized(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ApiUnauthorizedException();
            }
        }
        public async Task<Notices?> CreateAsync(Notices request)
        {
            AddToken();
            var response = await _httpClient.PostAsJsonAsync("api/Notices/",request);
            ThrowIfUnauthorized(response);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<Notices>();
        }

        
        public async Task<List<Notices>?>GetAllNoticesAsync()
        {
            AddToken();
            var response = await _httpClient.GetAsync("api/Notices");
            

            ThrowIfUnauthorized(response);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<List<Notices>>();
        }

        public async Task<Notices?> UpdateAsync(Notices request)
        {
            AddToken();
            var response = await _httpClient.PutAsJsonAsync($"api/Notices/{request.NoticeId}",request);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(
                $"UPDATE: {(int)response.StatusCode} {response.RequestMessage?.RequestUri} | {body}");

            ThrowIfUnauthorized(response);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<Notices>();
        }

        public async Task<bool> Delete(Notices request)
        {
            AddToken();
            var response = await _httpClient.DeleteAsync($"api/Notices/{request.NoticeId}");

            ThrowIfUnauthorized(response);

            return response.IsSuccessStatusCode;
        }

        public async Task<Notices?>GetNoticeByIdAsync(int id)
        {
            AddToken();
            var response = await _httpClient.GetAsync($"api/Notices/{id}");


            ThrowIfUnauthorized(response);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<Notices>();
        }


    }
}
