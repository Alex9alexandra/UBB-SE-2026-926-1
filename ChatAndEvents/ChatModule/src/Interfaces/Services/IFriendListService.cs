using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatModule.Models;


namespace ChatModule.src.Interfaces.Services
{
    public interface IFriendListService
    {
        Task<List<User>> GetFriendsAsync(Guid targetUserId);
        Task RemoveFriendAsync(Guid currentUserId, Guid targetFriendId);
    }
}
