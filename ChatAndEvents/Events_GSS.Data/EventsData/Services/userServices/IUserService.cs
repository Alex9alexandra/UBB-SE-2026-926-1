using Events_GSS.Data.Models;

namespace Events_GSS.Data.Services.userServices
{
    public interface IUserService
    {
        Task<User> GetCurrentUser();
        Task<User?> GetUserById(Guid userId);
        List<User> GetFriends(Guid userId);
        List<User> SearchFriends(Guid userId, string name);
        public Task<bool> IsAttending(Event currentEvent);
        public bool IsAdmin(Event currentEvent);
    }
}