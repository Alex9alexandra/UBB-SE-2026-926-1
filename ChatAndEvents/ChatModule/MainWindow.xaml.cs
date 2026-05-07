using BCrypt.Net;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.Database;
// --- MERGED TEAM NAMESPACES ---
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Repositories;
using ChatAndEvents.Data.EventsData.Repositories.achievementRepository;
using ChatAndEvents.Data.EventsData.Repositories.announcementRepository;
using ChatAndEvents.Data.EventsData.Repositories.categoriesRepository;
using ChatAndEvents.Data.EventsData.Repositories.discussionRepository;
using ChatAndEvents.Data.EventsData.Repositories.eventRepository;
using ChatAndEvents.Data.EventsData.Repositories.eventStatisticsRepository;
using ChatAndEvents.Data.EventsData.Repositories.notificationRepository;
using ChatAndEvents.Data.EventsData.Repositories.reputationRepository;
using ChatAndEvents.Data.EventsData.Services;
using ChatAndEvents.Data.EventsData.Services.achievementServices;
using ChatAndEvents.Data.EventsData.Services.announcementServices;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.categoryServices;
using ChatAndEvents.Data.EventsData.Services.discussionService;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Data.EventsData.Services.eventStatisticsServices;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using ChatAndEvents.Data.EventsData.Services.notificationServices;
using ChatAndEvents.Data.EventsData.Services.reputationService;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatModule.src.view_models;
using ChatModule.src.views;
using ChatModule.ViewModels;
using Events_GSS.ViewModels;
using Events_GSS.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace ChatModule
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }
        private readonly Guid _initialUserId;
        private readonly string _initialUsername;

        private readonly IUserRepository _userRepository;
        private readonly ConversationRepository _conversationRepository;
        private readonly ParticipantRepository _participantRepository;
        private readonly MessageRepository _messageRepository;
        private readonly DirectMessageService _directMessageService;
        private readonly IGroupService _groupService;
        private readonly SearchService _searchService;
        private readonly MessageService _messageService;
        private readonly MessageInteractionService _messageInteractionService;
        private readonly ReadReceiptService _readReceiptService;
        private readonly MentionService _mentionService;
        private readonly FriendRequestService _friendRequestService;
        private readonly BlockService _blockService;
        private readonly ProfileService _profileService;
        private readonly IMemberPanelService _memberPanelService;
        private readonly IModerationService _moderationService;

        private const string ConnectionString =
            "Data Source=.\\SQLEXPRESS;Initial Catalog=ChatAndEventsDB;" +
            "Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        public MainWindow()
            : this(Guid.Empty, "guest")
        {
        }

        public MainWindow(Guid userId, string username)
        {
            _initialUserId = userId;
            _initialUsername = username;

            var services = new ServiceCollection();

            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(ConnectionString),
                ServiceLifetime.Transient);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(ConnectionString),
                ServiceLifetime.Transient);

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IFriendRepository, FriendRepository>();
            services.AddTransient<FriendRepository>();
            services.AddTransient<IConversationRepository, ConversationRepository>();
            services.AddTransient<ConversationRepository>();
            services.AddTransient<IParticipantRepository, ParticipantRepository>();
            services.AddTransient<ParticipantRepository>();
            services.AddTransient<IMessageRepository, MessageRepository>();
            services.AddTransient<MessageRepository>();

            services.AddTransient<ConversationListService>();
            services.AddTransient<FriendRequestService>();
            services.AddTransient<FriendListService>();
            services.AddTransient<BlockService>();
            services.AddTransient<ProfileService>();
            services.AddTransient<DirectMessageService>();
            services.AddTransient<GroupService>();
            services.AddTransient<IGroupService, GroupService>();
            services.AddTransient<SearchService>();
            services.AddTransient<MessageService>();
            services.AddTransient<MessageInteractionService>();
            services.AddTransient<ReadReceiptService>();
            services.AddTransient<MentionService>();
            services.AddTransient<MemberPanelService>();
            services.AddTransient<IMemberPanelService, MemberPanelService>();
            services.AddTransient<ModerationService>();
            services.AddTransient<IModerationService, ModerationService>();

            services.AddTransient<IEventRepository, EventRepository>();
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<IQuestRepository, QuestRepository>();
            services.AddTransient<IQuestMemoryRepository, QuestMemoryRepository>();
            services.AddTransient<IAnnouncementRepository, AnnouncementRepository>();
            services.AddTransient<IDiscussionRepository, DiscussionRepository>();
            services.AddTransient<IMemoryRepository, MemoryRepository>();
            services.AddTransient<IAttendedEventRepository, AttendedEventRepository>();
            services.AddTransient<INotificationRepository, NotificationRepository>();
            services.AddTransient<IReputationRepository, ReputationRepository>();
            services.AddTransient<IAchievementRepository, AchievementRepository>();
            services.AddTransient<IEventStatisticsRepository, EventStatisticsRepository>();

            services.AddTransient<IEventService, EventService>();
            services.AddTransient<ICategoryServices, CategoryServices>();
            services.AddTransient<IQuestService, QuestService>();
            services.AddTransient<IQuestApprovalService, QuestApprovalService>();
            services.AddTransient<IAnnouncementService, AnnouncementService>();
            services.AddTransient<IDiscussionService, DiscussionService>();
            services.AddTransient<IMemoryService, MemoryService>();
            services.AddTransient<IAttendedEventService, AttendedEventService>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddSingleton<IReputationService, ReputationService>();
            services.AddTransient<IAchievementService, AchievementService>();
            services.AddTransient<IEventStatisticsService, EventStatisticsService>();

            services.AddTransient<EventListingViewModel>();
            services.AddTransient<ReputationViewModel>();
            services.AddTransient<NotificationViewModel>();

            services.AddSingleton<IUserService>(sp =>
            {
                var chatUserService = new ChatUserService(
                    sp.GetRequiredService<IUserRepository>(),
                    sp.GetRequiredService<IReputationRepository>(),
                    sp.GetRequiredService<IAttendedEventService>(),
                    sp.GetRequiredService<IDbContextFactory<AppDbContext>>()
                );
                chatUserService.SetCurrentUserId(userId);
                return chatUserService;
            });

            Events_GSS.App.Services = services.BuildServiceProvider();
            Events_GSS.App.MainWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);

            var provider = Events_GSS.App.Services;

            _userRepository = provider.GetRequiredService<IUserRepository>();
            _conversationRepository = provider.GetRequiredService<ConversationRepository>();
            _participantRepository = provider.GetRequiredService<ParticipantRepository>();
            _messageRepository = provider.GetRequiredService<MessageRepository>();
            _directMessageService = provider.GetRequiredService<DirectMessageService>();
            _groupService = provider.GetRequiredService<IGroupService>();
            _searchService = provider.GetRequiredService<SearchService>();
            _messageService = provider.GetRequiredService<MessageService>();
            _messageInteractionService = provider.GetRequiredService<MessageInteractionService>();
            _readReceiptService = provider.GetRequiredService<ReadReceiptService>();
            _mentionService = provider.GetRequiredService<MentionService>();
            _friendRequestService = provider.GetRequiredService<FriendRequestService>();
            _blockService = provider.GetRequiredService<BlockService>();
            _profileService = provider.GetRequiredService<ProfileService>();
            _memberPanelService = provider.GetRequiredService<IMemberPanelService>();
            _moderationService = provider.GetRequiredService<IModerationService>();

            var conversationListService = provider.GetRequiredService<ConversationListService>();
            var friendListService = provider.GetRequiredService<FriendListService>();

            
            ViewModel = new MainViewModel(
                conversationListService,
                _friendRequestService,
                friendListService,
                _blockService,
                _profileService,
                _directMessageService,
                provider.GetRequiredService<IEventRepository>(),
                provider.GetRequiredService<INotificationService>(),
                provider.GetRequiredService<IReputationService>(),
                provider.GetRequiredService<IUserService>(),
                provider.GetRequiredService<IEventService>(),
                provider.GetRequiredService<IQuestService>(),
                provider.GetRequiredService<IAttendedEventService>(),
                provider.GetRequiredService<IAchievementService>());

            InitializeComponent();

            ViewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.CurrentPage))
                {
                    SafeRenderCurrentPage();
                }
            };
            ViewModel.NavigateToChatRequested += conversationId => _ = OpenChatAsync(conversationId);

            ViewModel.NavigateToLoginRequested += () =>
            {
                
                var loginServices = new ServiceCollection();
                loginServices.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(ConnectionString), ServiceLifetime.Transient);
                loginServices.AddTransient<IUserRepository, UserRepository>();
                loginServices.AddTransient<AuthenticationService>();
                var loginProvider = loginServices.BuildServiceProvider();

                var loginWindow = new LoginWindow(loginProvider.GetRequiredService<AuthenticationService>());
                loginWindow.LoginSucceeded += (newUserId, newUsername) =>
                {
                    var nextMain = new MainWindow(newUserId, newUsername);
                    App.SetMainWindow(nextMain);
                    nextMain.Activate();
                    loginWindow.Close();
                    Close();
                    return Task.CompletedTask;
                };
                loginWindow.Activate();
            };

            _ = InitialiseAndRenderAsync();
        }

        
        private async System.Threading.Tasks.Task InitialiseAndRenderAsync()
        {
            try
            {
                await ViewModel.InitialiseAsync(_initialUserId, _initialUsername);
                SafeRenderCurrentPage();
            }
            catch (Exception ex)
            {
                if (CurrentPageHost.XamlRoot != null)
                {
                    await ShowInfoDialogAsync("Startup error", ex.Message);
                }
            }
        }

        private void RenderCurrentPage()
        {
            object? view = ViewModel.CurrentPage switch
            {
                ConversationListViewModel vm => BuildConversationListView(vm),
                FriendListViewModel vm => new FriendListView(vm),
                FriendRequestsViewModel vm => new FriendRequestsView(vm),
                ProfileViewModel vm => BuildProfileView(vm),
                ChatViewModel vm => new ChatView(vm),

                EventListingViewModel vm => new EventListingPage(vm),
                ReputationViewModel vm => new ReputationPage(vm),
                NotificationViewModel => new NotificationView(),
                CreateEventViewModel vm => new CreateEventPage(),
                EventDetailViewModel vm => new EventDetailPage(vm),
                MyEventsViewModel vm => new MyEventsPage(vm),

                _ => null
            };

            CurrentPageHost.Content = view;
        }

        private void SafeRenderCurrentPage()
        {
            try
            {
                RenderCurrentPage();
            }
            catch (Exception ex)
            {
                CurrentPageHost.Content = new TextBlock
                {
                    Text = $"Failed to render page: {ex.Message}",
                    Margin = new Thickness(16)
                };
            }
        }

        private ConversationListView BuildConversationListView(ConversationListViewModel vm)
        {
            vm.NewGroupRequested -= OnNewGroupRequested;
            vm.NewDmRequested -= OnNewDmRequested;
            vm.ConversationOpened -= OnConversationOpened;

            vm.NewGroupRequested += OnNewGroupRequested;
            vm.NewDmRequested += OnNewDmRequested;
            vm.ConversationOpened += OnConversationOpened;

            return new ConversationListView(vm);
        }

        private ProfileView BuildProfileView(ProfileViewModel vm)
        {
            vm.NavigateToChatRequested -= OnConversationOpened;
            vm.NavigateToChatRequested += OnConversationOpened;
            return new ProfileView(vm);
        }

        private void OnConversationOpened(Guid conversationId) => _ = OpenChatAsync(conversationId);
        private void OnNewGroupRequested() => _ = ShowCreateGroupDialogAsync();
        private void OnNewDmRequested() => _ = ShowCreateDmDialogAsync();

        private async Task ShowCreateGroupDialogAsync()
        {
            var createGroupViewModel = new CreateGroupViewModel(_groupService, _searchService, ViewModel.CurrentUserId);
            var dialog = new CreateGroupDialog(createGroupViewModel)
            {
                XamlRoot = CurrentPageHost.XamlRoot
            };

            _ = await dialog.ShowAsync();

            if (dialog.CreatedConversation != null)
            {
                await OpenChatAsync(dialog.CreatedConversation.Id);
            }
        }

        private async Task ShowCreateDmDialogAsync()
        {
            if (CurrentPageHost.XamlRoot == null) return;

            var usernameBox = new TextBox
            {
                PlaceholderText = "Enter username",
                Margin = new Thickness(0, 8, 0, 0)
            };

            var dialog = new ContentDialog
            {
                Title = "Start New DM",
                Content = usernameBox,
                PrimaryButtonText = "Start",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = CurrentPageHost.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            var username = usernameBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(username)) return;

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || user.Id == ViewModel.CurrentUserId)
            {
                await ShowInfoDialogAsync("User not found", "Enter another username to start a DM.");
                return;
            }

            var conversation = await _directMessageService.GetOrCreateAsync(ViewModel.CurrentUserId, user.Id);
            await OpenChatAsync(conversation.Id);
        }

        private async Task ShowInfoDialogAsync(string title, string message)
        {
            if (CurrentPageHost.XamlRoot == null) return;

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = CurrentPageHost.XamlRoot
            };

            _ = await dialog.ShowAsync();
        }

        private async Task<string?> ShowInputDialogAsync(string title, string placeholder)
        {
            var inputBox = new TextBox
            {
                PlaceholderText = placeholder,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var dialog = new ContentDialog
            {
                Title = title,
                Content = inputBox,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = CurrentPageHost.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return null;

            return inputBox.Text;
        }

        private async Task OpenChatAsync(Guid conversationId)
        {
            try
            {
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null) return;

                var chatViewModel = new ChatViewModel(
                    _messageService,
                    _messageInteractionService,
                    _readReceiptService,
                    _mentionService,
                    _directMessageService,
                    _conversationRepository,
                    _searchService,
                    ViewModel.CurrentUserId);

                await chatViewModel.LoadAsync(conversationId);

                var chatView = new ChatView(chatViewModel);

                chatViewModel.LeaveGroupRequested += async () =>
                {
                    try
                    {
                        await _groupService.LeaveGroupAsync(conversationId, ViewModel.CurrentUserId);
                        await ShowInfoDialogAsync("Group", "You left the group.");
                        ViewModel.GoToConversationsCommand.Execute(null);
                    }
                    catch (Exception ex)
                    {
                        await ShowInfoDialogAsync("Unable to leave group", ex.Message);
                    }
                };

                chatViewModel.SetNicknameRequested += async () =>
                {
                    var nickname = await ShowInputDialogAsync("Set group nickname", "Nickname (max 16 chars)");
                    if (nickname == null) return;

                    try
                    {
                        await _messageService.SetNicknameAsync(conversationId, ViewModel.CurrentUserId, nickname);
                        await chatViewModel.LoadAsync(conversationId);
                    }
                    catch (Exception ex)
                    {
                        await ShowInfoDialogAsync("Nickname", ex.Message);
                    }
                };

                chatViewModel.ClearNicknameRequested += async () =>
                {
                    try
                    {
                        await _messageService.SetNicknameAsync(conversationId, ViewModel.CurrentUserId, null);
                        await chatViewModel.LoadAsync(conversationId);
                    }
                    catch (Exception ex)
                    {
                        await ShowInfoDialogAsync("Nickname", ex.Message);
                    }
                };

                if (conversation.Type == ConversationType.Group)
                {
                    var memberPanelViewModel = new MemberPanelViewModel(_memberPanelService, _moderationService, ViewModel.CurrentUserId);
                    memberPanelViewModel.NavigateToProfileRequested += async userId =>
                    {
                        var profileVm = new ProfileViewModel(_friendRequestService, _blockService, _directMessageService, _profileService, ViewModel.CurrentUserId);
                        await profileVm.LoadAsync(userId);
                        var profilePanelVm = new ConversationSidePanelViewModel(ConversationType.Dm, profileVm, () =>
                        {
                            var membersPanelVm = new ConversationSidePanelViewModel(ConversationType.Group, memberPanelViewModel);
                            chatView.SetSidePanel(new ConversationSidePanelView(membersPanelVm));
                        });
                        chatView.SetSidePanel(new ConversationSidePanelView(profilePanelVm));
                    };
                    await memberPanelViewModel.LoadAsync(conversationId);
                    var sideVm = new ConversationSidePanelViewModel(ConversationType.Group, memberPanelViewModel);
                    chatView.SetSidePanel(new ConversationSidePanelView(sideVm));
                }
                else
                {
                    var otherUser = await _directMessageService.GetOtherUserAsync(conversationId, ViewModel.CurrentUserId);
                    if (otherUser != null)
                    {
                        var profileVm = new ProfileViewModel(_friendRequestService, _blockService, _directMessageService, _profileService, ViewModel.CurrentUserId);
                        await profileVm.LoadAsync(otherUser.Id);
                        var sideVm = new ConversationSidePanelViewModel(ConversationType.Dm, profileVm);
                        chatView.SetSidePanel(new ConversationSidePanelView(sideVm));
                    }
                }

                CurrentPageHost.Content = chatView;
            }
            catch (InvalidOperationException ex)
            {
                await ShowInfoDialogAsync("Unable to open conversation", ex.Message);
                ViewModel.GoToConversationsCommand.Execute(null);
            }
        }
    }
}