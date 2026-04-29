using Events_GSS.Data.Models;

namespace Events_GSS.Services.Interfaces
{
    public interface IUserService
    {
        User GetCurrentUser();
        User? GetUserById(Guid userId);
        List<User> GetFriends(Guid userId);
        List<User> SearchFriends(Guid userId, string name);
        public Task<bool> IsAttending(Event currentEvent);
        public bool IsAdmin(Event currentEvent);
    }
}