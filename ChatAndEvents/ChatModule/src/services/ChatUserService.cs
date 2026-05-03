using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Repositories.reputationRepository;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatModule.Services;
using Events_GSS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ChatUserService : IUserService
{
    private readonly IUserRepository chatUserRepo;
    private readonly IReputationRepository reputationRepo;
    private readonly IAttendedEventService attendedEventService;

    private Guid currentUserId;

    public ChatUserService(
        IUserRepository chatUserRepo,
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

    public async Task<ChatAndEvents.Data.EventsData.Models.User> GetCurrentUser()
    {
        var chatUser = await chatUserRepo.GetByIdAsync(currentUserId);
        if (chatUser == null) throw new Exception("User not found");

        var reputationScore = await reputationRepo.GetReputationScoreAsync(chatUser.Id);

        return new ChatAndEvents.Data.EventsData.Models.User
        {
            UserId = chatUser.Id,
            Name = chatUser.Username,
            ReputationPoints = reputationScore.ReputationPoints,
            ReputationScore = reputationScore,
        };
    }

    public async Task<ChatAndEvents.Data.EventsData.Models.User?> GetUserById(Guid userId)
    {
        var chatUser = await chatUserRepo.GetByIdAsync(userId);
        if (chatUser == null) return null;

        var reputationScore = await reputationRepo.GetReputationScoreAsync(userId);

        return new ChatAndEvents.Data.EventsData.Models.User
        {
            UserId = chatUser.Id,
            Name = chatUser.Username,
            ReputationPoints = reputationScore.ReputationPoints,
            ReputationScore = reputationScore,
        };
    }

    public List<ChatAndEvents.Data.EventsData.Models.User> GetFriends(Guid userId) => new();
    public List<ChatAndEvents.Data.EventsData.Models.User> SearchFriends(Guid userId, string name) => new();

    public async Task<bool> IsAttending(Event currentEvent)
    {
        var attendingEvents = await attendedEventService.GetAttendedEventsAsync(currentUserId);
        return attendingEvents.Any(ae => ae.Event.EventId == currentEvent.EventId);
    }

    public bool IsAdmin(Event currentEvent)
    {
        return currentEvent.Admin.UserId == currentUserId;
    }
}
