using System;
using System.Threading.Tasks;
using ChatModule.Models;

namespace ChatModule.Services
{
    public interface IAuthenticationService
    {
        Task ChangePasswordAsync(string email, string newPassword);
        Task<User?> LoginAsync(string username, string password);
        Task<User> RegisterAsync(string username, string email, string password, string phone, DateTime? birthday, string? avatarUrl);
    }
}