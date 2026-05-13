//https://localhost:7283-> port for https -> look in properties  for the current web app

using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.EventsData.Services.achievementServices;
using ChatAndEvents.Data.EventsData.Services.announcementServices;
using ChatAndEvents.Data.EventsData.Services.reputationService;
using ChatAndEvents.Data.EventsData.Services.userServices;


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

builder.Services.AddScoped<IReputationService, ReputationHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ReputationHttpService(factory.CreateClient("API"));
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

// Add services to the container.
builder.Services.AddControllersWithViews();


//cookie authentification
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

//register the http services - this is just an example
//builder.Services.AddScoped<IEventService, EventHttpService>(sp =>
//{
//    var factory = sp.GetRequiredService<IHttpClientFactory>();
//    return new EventHttpService(factory.CreateClient("API"));
//});
//repeat ^ for the all services

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
