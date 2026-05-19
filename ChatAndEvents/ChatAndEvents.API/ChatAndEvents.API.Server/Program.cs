using ChatAndEvents.Data;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.Database;
using Events_GSS.Data.Repositories;
using Events_GSS.Data.Repositories.achievementRepository;
using Events_GSS.Data.Repositories.announcementRepository;
using Events_GSS.Data.Repositories.categoriesRepository;
using Events_GSS.Data.Repositories.discussionRepository;
using Events_GSS.Data.Repositories.eventRepository;
using Events_GSS.Data.Repositories.eventStatisticsRepository;
using Events_GSS.Data.Repositories.notificationRepository;
using Events_GSS.Data.Repositories.reputationRepository;
using Events_GSS.Data.Services;
using Events_GSS.Data.Services.achievementServices;
using Events_GSS.Data.Services.announcementServices;
using Events_GSS.Data.Services.attendedEventServices;
using Events_GSS.Data.Services.categoryServices;
using Events_GSS.Data.Services.discussionService;
using Events_GSS.Data.Services.eventServices;
using Events_GSS.Data.Services.eventStatisticsServices;
using Events_GSS.Data.Services.Interfaces;
using Events_GSS.Data.Services.notificationServices;
using Events_GSS.Data.Services.reputationService;
using Events_GSS.Data.Services.userServices;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ChatAndEventsDB") ?? throw new InvalidOperationException("Connection string 'ChatAndEventsDB' not found.")));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // This tells Swagger to use the full path (e.g. ChatData.domain.User) 
    // to prevent naming collisions!
    options.CustomSchemaIds(type => type.FullName);
});

// chat module repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

// chat module services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IConversationListService, ConversationListService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IDirectMessageService, DirectMessageService>();
builder.Services.AddScoped<IBlockService, BlockService>();
builder.Services.AddScoped<IFriendListService, FriendListService>();
builder.Services.AddScoped<IFriendRequestService, FriendRequestService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IMemberPanelService, MemberPanelService>();
builder.Services.AddScoped<IMentionService, MentionService>();
builder.Services.AddScoped<IMessageInteractionService, MessageInteractionService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IModerationService, ModerationService>();
builder.Services.AddScoped<IReadReceiptService, ReadReceiptService>();

// events module repositories
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IQuestRepository, QuestRepository>();
builder.Services.AddScoped<IQuestMemoryRepository, QuestMemoryRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IDiscussionRepository, DiscussionRepository>();
builder.Services.AddScoped<IMemoryRepository, MemoryRepository>();
builder.Services.AddScoped<IAttendedEventRepository, AttendedEventRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IReputationRepository, ReputationRepository>();
builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();
builder.Services.AddScoped<IEventStatisticsRepository, EventStatisticsRepository>();

// events module services
builder.Services.AddScoped<IUserService, ChatUserService>();
builder.Services.AddScoped<IMemoryService, MemoryService>();
builder.Services.AddScoped<IQuestApprovalService, QuestApprovalService>();
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEventStatisticsService, EventStatisticsService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IDiscussionService, DiscussionService>();
builder.Services.AddScoped<ICategoryServices, CategoryServices>();
builder.Services.AddScoped<IAttendedEventService, AttendedEventService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IReputationService, ReputationService>();

var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();
app.Run();
