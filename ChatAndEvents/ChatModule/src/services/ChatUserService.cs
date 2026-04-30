using ChatModule.Models;
using ChatModule.Repositories;
using ChatModule.Services;
using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories.reputationRepository;
using Events_GSS.Services;
using Events_GSS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ChatUserService : IUserService
{
    private readonly ChatModule.Repositories.IUserRepository chatUserRepo;
    private readonly IReputationRepository reputationRepo;
    private readonly IAttendedEventService attendedEventService;

    private Guid currentUserId;

    public ChatUserService(
        ChatModule.Repositories.IUserRepository chatUserRepo,
        IReputationRepository reputationRepo,
        IAttendedEventService attendedEventService)
    {
        this.chatUserRepo = chatUserRepo;
        this.reputationRepo = reputationRepo;
        this.attendedEventService = attendedEventService;
    }

    public void SetCurrentUserId(Guid userId)
    {
        currentUserId = userId;
    }

    public Events_GSS.Data.Models.User GetCurrentUser()
    {
        var chatUser = chatUserRepo.GetByIdAsync(currentUserId).Result;
        if (chatUser == null) throw new Exception("User not found");

        int rep = reputationRepo.GetReputationPointsAsync(chatUser.Id).Result;

        return new Events_GSS.Data.Models.User
        {
            UserId = chatUser.Id,
            Name = chatUser.Username,
            ReputationPoints = rep
        };
    }

    public Events_GSS.Data.Models.User? GetUserById(Guid userId)
    {
        var chatUser = chatUserRepo.GetByIdAsync(userId).Result;
        if (chatUser == null) return null;

        int rep = reputationRepo.GetReputationPointsAsync(userId).Result;

        return new Events_GSS.Data.Models.User
        {
            UserId = chatUser.Id,
            Name = chatUser.Username,
            ReputationPoints = rep
        };
    }

    public List<Events_GSS.Data.Models.User> GetFriends(Guid userId) => new();
    public List<Events_GSS.Data.Models.User> SearchFriends(Guid userId, string name) => new();

    public async Task<bool> IsAttending(Events_GSS.Data.Models.Event currentEvent)
    {
        var attendingEvents = await attendedEventService.GetAttendedEventsAsync(currentUserId);
        return attendingEvents.Any(ae => ae.Event.EventId == currentEvent.EventId);
    }

    public bool IsAdmin(Events_GSS.Data.Models.Event currentEvent)
    {
        return currentEvent.Admin.UserId == currentUserId;
    }
}