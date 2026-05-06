using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Repositories.reputationRepository;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatModule.Services;
using Events_GSS.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ChatUserService : IUserService
{
    private readonly IUserRepository chatUserRepo;
    private readonly IReputationRepository reputationRepo;
    private readonly IAttendedEventService attendedEventService;
    private readonly AppDbContext _db;
    private Guid currentUserId;

    public ChatUserService(
        IUserRepository chatUserRepo,
        IReputationRepository reputationRepo,
        IAttendedEventService attendedEventService,
        AppDbContext db)
    {
        this.chatUserRepo = chatUserRepo;
        this.reputationRepo = reputationRepo;
        this.attendedEventService = attendedEventService;
        this._db = db;
    }

    public void SetCurrentUserId(Guid userId)
    {
        currentUserId = userId;
    }

    public async Task<ChatAndEvents.Data.EventsData.Models.User> GetCurrentUser()
    {
        var reputationScore = await reputationRepo.GetReputationScoreAsync(currentUserId);

        var eventsUser = await _db.Set<ChatAndEvents.Data.EventsData.Models.User>()
            .FirstOrDefaultAsync(u => u.UserId == currentUserId);

        if (eventsUser == null) throw new Exception("User not found");

        eventsUser.ReputationPoints = reputationScore?.ReputationPoints ?? 0;
        eventsUser.ReputationScore = reputationScore;

        return eventsUser;
    }

    public async Task<ChatAndEvents.Data.EventsData.Models.User?> GetUserById(Guid userId)
    {
        var reputationScore = await reputationRepo.GetReputationScoreAsync(userId);

        var eventsUser = await _db.Set<ChatAndEvents.Data.EventsData.Models.User>()
            .FirstOrDefaultAsync(u => u.UserId == userId);


        if (eventsUser == null) return null;

        eventsUser.ReputationPoints = reputationScore?.ReputationPoints ?? 0;
        eventsUser.ReputationScore = reputationScore;

        return eventsUser;
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
