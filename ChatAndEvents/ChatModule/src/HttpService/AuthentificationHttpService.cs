using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.services;

namespace ChatModule.HttpService
{
    public class AuthentificationHttpService : IAuthentificationService
    {
        private readonly HttpClient _httpClient;

        public AuthentificationHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsync(
                $"api/Authentication/login?username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}",
                null);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<User>();
        }

        public async Task<User> RegisterAsync(
            string username,
            string email,
            string password,
            string phone,
            DateTime? birthday,
            string? avatarUrl)
        {
            var response = await _httpClient.PostAsync(
                $"api/Authentication/register?username={Uri.EscapeDataString(username)}&email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}&phone={Uri.EscapeDataString(phone)}&birthday={birthday:O}&avatarUrl={Uri.EscapeDataString(avatarUrl ?? string.Empty)}",
                null);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<User>()
                   ?? throw new InvalidOperationException("Failed to read user from API.");
        }

        public async Task ChangePasswordAsync(string email, string newPassword)
        {
            var response = await _httpClient.PutAsync(
                $"api/Authentication/password?email={Uri.EscapeDataString(email)}&newPassword={Uri.EscapeDataString(newPassword)}",
                null);

            response.EnsureSuccessStatusCode();
        }
    }
}

