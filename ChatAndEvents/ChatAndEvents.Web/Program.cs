//https://localhost:7283-> port for https -> look in properties  for the current web app


using System;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using Events_GSS.Data.Services.eventServices;
using Events_GSS.Data.Services; 
using Events_GSS.Data.Services.achievementServices;
using Events_GSS.Data.Services.announcementServices;
using Events_GSS.Data.Services.Interfaces; 
using Events_GSS.Data.Services.reputationService;
using Events_GSS.Data.Services.userServices;

using AuthHttp = ChatAndEvents.Data.ChatData.services.AuthentificationHttpService;

// using RepHttp = Events_GSS.Data.Services.reputationService.ReputationHttpService;

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

builder.Services.AddSingleton(new CurrentUserContext(Guid.Parse("11111111-1111-1111-1111-111111111111")));

builder.Services.AddScoped<IAnnouncementService, AnnouncementHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new AnnouncementHttpService(factory.CreateClient("API"));
});

// Register IReputationService by resolving the concrete type at runtime using an assembly-qualified name
builder.Services.AddScoped<IReputationService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    // Specify the assembly that contains the desired implementation. Adjust "ChatModule" if needed.
    var implType = Type.GetType("Events_GSS.Data.Services.reputationService.ReputationHttpService, ChatModule");
    if (implType == null)
    {
        // fallback: try ChatAndEvents.Data assembly
        implType = Type.GetType("Events_GSS.Data.Services.reputationService.ReputationHttpService, ChatAndEvents.Data");
    }
    if (implType == null)
        throw new InvalidOperationException("ReputationHttpService type not found in referenced assemblies. Remove duplicate definitions or adjust the assembly-qualified name.");

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

builder.Services.AddScoped<IAuthentificationService, AuthHttp>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new AuthHttp(factory.CreateClient("API"));
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
    client.BaseAddress = new Uri("https://localhost:7305/"); 
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
