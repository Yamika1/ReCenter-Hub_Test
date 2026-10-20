using ReCenterHub.Models;
using System.Net;
using System.Net.Http.Headers;

namespace ReCenterHub.Services
{
    public class WorkshopBookingService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkshopBookingService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
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

        private static void ThrowIfUnauthorized(
            HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ApiUnauthorizedException();
            }
        }

        public async Task<WorkshopBooking?> CreateAsync(
            WorkshopBooking request)
        {
            AddToken();

            var response = await _httpClient.PostAsJsonAsync(
                "api/WorkshopBooking/",
                request);

            ThrowIfUnauthorized(response);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<WorkshopBooking>();
        }

        public async Task<List<WorkshopBooking>?>
            GetAllWorkshopBookingsAsync()
        {
            AddToken();

            var response = await _httpClient.GetAsync(
                "api/WorkshopBooking");

            ThrowIfUnauthorized(response);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<List<WorkshopBooking>>();
        }

        public async Task<WorkshopBooking?> UpdateAsync(WorkshopBooking wb)
        {
            AddToken();
            var response = await _httpClient.PutAsJsonAsync(
                $"api/WorkshopBooking/{wb.WorkshopBookingID}", wb);

            ThrowIfUnauthorized(response);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<WorkshopBooking>();
        }

        

        public async Task<bool> DeleteAsync(int? id)
        {
            AddToken();

            var response = await _httpClient.DeleteAsync(
                $"api/WorkshopBooking/{id}");

            ThrowIfUnauthorized(response);

            return response.IsSuccessStatusCode;
        }

        public async Task<WorkshopBooking?> GetWorkshopBookingByIdAsync(int id)
        {
            AddToken();

            var response = await _httpClient.GetAsync(
                $"api/WorkshopBooking/{id}");

            ThrowIfUnauthorized(response);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<WorkshopBooking>();
        }

        public List<WorkshopBooking> UpcomingSessions(
            IEnumerable<WorkshopBooking> bookings)
        {
            return bookings
                .Where(b =>
                    b.Date_and_Time > DateTime.Now &&
                    (b.Status == "Scheduled" ||
                     b.Status == "Rescheduled"))
                .ToList();
        }

        public async Task<List<WorkshopBooking>?>
            GetClientWorkshopsAsync()
        {
            AddToken();

            var response = await _httpClient.GetAsync(
                "api/WorkshopBooking/client-workshops");

            ThrowIfUnauthorized(response);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<List<WorkshopBooking>>();
        }
    }
}