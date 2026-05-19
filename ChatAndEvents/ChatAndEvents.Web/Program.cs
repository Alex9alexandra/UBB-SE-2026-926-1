//https://localhost:7283-> port for https -> look in properties for the current web app

using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using Events_GSS.Data.Services;
using Events_GSS.Data.Services.achievementServices;
using Events_GSS.Data.Services.announcementServices;
using Events_GSS.Data.Services.eventServices;
using Events_GSS.Data.Services.Interfaces;
using Events_GSS.Data.Services.notificationServices;
using Events_GSS.Data.Services.reputationService;
using Events_GSS.Data.Services.userServices;
using System;

// Namespace-urile pentru servicii aduse de pe server adaptate sau păstrate pentru compatibilitate
using ChatAndEvents.Data.EventsData.Services.memoryServices;
using ChatAndEvents.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IMessageService, MessageHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new MessageHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IConversationListService, ConversationListHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ConversationListHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IReadReceiptService, ReadReceiptHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ReadReceiptHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IMemberPanelService, MemberPanelHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new MemberPanelHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IModerationService, ModerationHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ModerationHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<ISearchService, SearchHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new SearchHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IFriendListService, FriendListApiClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new FriendListHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IFriendRequestService, FriendRequestHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new FriendRequestHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IProfileService, ProfileHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ProfileHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IBlockService, BlockHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new BlockHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IDirectMessageService, DirectMessageHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new DirectMessageHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IDiscussionService, DiscussionHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new DiscussionHttpService(factory.CreateClient("API"));
});

builder.Services.AddSingleton(new CurrentUserContext(Guid.Parse("11111111-1111-1111-1111-111111111111")));

builder.Services.AddScoped<IAnnouncementService, AnnouncementHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new AnnouncementHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IReputationService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var implType = Type.GetType("Events_GSS.Data.Services.reputationService.ReputationHttpService, ChatModule")
                   ?? Type.GetType("Events_GSS.Data.Services.reputationService.ReputationHttpService, ChatAndEvents.Data");

    if (implType == null)
        throw new InvalidOperationException("ReputationHttpService type not found in referenced assemblies.");

    return (IReputationService)Activator.CreateInstance(implType, factory.CreateClient("API"));
});

builder.Services.AddScoped<IAchievementService, AchievementHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new AchievementHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IUserService, UserHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var currentUserContext = sp.GetRequiredService<CurrentUserContext>();
    return new UserHttpService(factory.CreateClient("API"), currentUserContext);
});

builder.Services.AddScoped<IAuthenticationService, AuthenticationHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new AuthenticationHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IMemoryService, MemoryHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new MemoryHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IEventService, EventHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new EventHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<INotificationService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var implType = Type.GetType("Events_GSS.Data.Services.notificationServices.NotificationHttpService, ChatAndEvents.Data")
                   ?? Type.GetType("Events_GSS.Data.Services.notificationServices.NotificationHttpService, ChatModule");

    if (implType == null)
        throw new InvalidOperationException("NotificationHttpService type not found in referenced assemblies.");

    return (INotificationService)Activator.CreateInstance(implType, factory.CreateClient("API"));
});

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("API", client =>
{
    var apiBaseAddress = builder.Configuration["Api:BaseAddress"] ?? "https://localhost:7305/";
    client.BaseAddress = new Uri(apiBaseAddress);
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();