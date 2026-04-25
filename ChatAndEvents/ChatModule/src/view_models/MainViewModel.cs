using System;
using System.Threading.Tasks;
using ChatModule.Services;
using ChatModule.src.view_models;
using Events_GSS.Data.Repositories.eventRepository;
using Events_GSS.Data.Repositories.notificationRepository;
using Events_GSS.Data.Repositories.reputationRepository;
using Events_GSS.Services.Interfaces; // For IUserService
using Events_GSS.ViewModels;
using Events_GSS.Data.Services.notificationServices;
using Events_GSS.Data.Services.reputationService;

namespace ChatModule.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ConversationListService _conversationListService;
        private readonly FriendRequestService _friendRequestService;
        private readonly FriendListService? _friendListService;
        private readonly BlockService _blockService;
        private readonly IDirectMessageService _directMessageService;
        private readonly IProfileService _profileService;

        private readonly IEventRepository _eventRepository;
        private readonly INotificationService _notificationService;
        private readonly IReputationService _reputationService;
        private readonly IUserService _userService;

        private Guid _currentUserId;
        public Guid CurrentUserId
        {
            get => _currentUserId;
            private set => Set(ref _currentUserId, value);
        }

        private string _currentUsername = string.Empty;
        public string CurrentUsername
        {
            get => _currentUsername;
            private set => Set(ref _currentUsername, value);
        }

        private object? _currentPage;
        public object? CurrentPage
        {
            get => _currentPage;
            private set => Set(ref _currentPage, value);
        }

        public event Action? NavigateToLoginRequested;
        public event Action<Guid>? NavigateToChatRequested;

        // --- ORIGINAL COMMANDS ---
        public RelayCommand GoToConversationsCommand { get; }
        public RelayCommand GoToFriendsCommand { get; }
        public RelayCommand GoToProfileCommand { get; }
        public RelayCommand LogoutCommand { get; }

        // --- NEW MERGED COMMANDS ---
        public RelayCommand GoToEventsCommand { get; }
        public RelayCommand GoToMyEventsCommand { get; }
        public RelayCommand GoToReputationCommand { get; }
        public RelayCommand GoToNotificationsCommand { get; }

        // Constructor 1 (Without FriendListService)
public MainViewModel(
    ConversationListService conversationListService,
    FriendRequestService friendRequestService,
    BlockService blockService,
    IProfileService profileService,
    IDirectMessageService directMessageService,
    // --- UPDATED TO USE SERVICES ---
    IEventRepository eventRepository,
    INotificationService notificationService,
    IReputationService reputationService,
    IUserService userService)
    : this(
        conversationListService,
        friendRequestService,
        friendListService: null, // This is the only difference
        blockService,
        profileService,
        directMessageService,
        eventRepository,
        notificationService,
        reputationService,
        userService)
{
}

// Constructor 2 (The Main One)
public MainViewModel(
    ConversationListService conversationListService,
    FriendRequestService friendRequestService,
    FriendListService? friendListService,
    BlockService blockService,
    IProfileService profileService,
    IDirectMessageService directMessageService,
    // --- UPDATED TO USE SERVICES ---
    IEventRepository eventRepository,
    INotificationService notificationService,
    IReputationService reputationService,
    IUserService userService)
{
    _conversationListService = conversationListService ?? throw new ArgumentNullException(nameof(conversationListService));
    _friendRequestService = friendRequestService ?? throw new ArgumentNullException(nameof(friendRequestService));
    _friendListService = friendListService;
    _blockService = blockService ?? throw new ArgumentNullException(nameof(blockService));
    _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
    _directMessageService = directMessageService ?? throw new ArgumentNullException(nameof(directMessageService));

    // Assign the new services
    _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
    _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    _reputationService = reputationService ?? throw new ArgumentNullException(nameof(reputationService));
    _userService = userService ?? throw new ArgumentNullException(nameof(userService));

    // Initialize Original Commands
    GoToConversationsCommand = new RelayCommand(GoToConversationsAsync);
    GoToFriendsCommand = new RelayCommand(GoToFriendsAsync);
    GoToProfileCommand = new RelayCommand(GoToProfileAsync);
    LogoutCommand = new RelayCommand(LogoutAsync);

    // Initialize New Merged Commands
    GoToEventsCommand = new RelayCommand(GoToEventsAsync);
    GoToMyEventsCommand = new RelayCommand(GoToMyEventsAsync);
    GoToReputationCommand = new RelayCommand(GoToReputationAsync);
    GoToNotificationsCommand = new RelayCommand(GoToNotificationsAsync);
}

        public Task InitialiseAsync(Guid userId, string username)
        {
            CurrentUserId = userId;
            CurrentUsername = username;
            CurrentPage = new ConversationListViewModel(_conversationListService, CurrentUserId);
            return Task.CompletedTask;
        }

        private Task GoToConversationsAsync()
        {
            CurrentPage = new ConversationListViewModel(_conversationListService, CurrentUserId);
            return Task.CompletedTask;
        }

        private Task GoToFriendsAsync()
        {
            if (_friendListService != null)
            {
                var friendListViewModel = new FriendListViewModel(_friendListService, _friendRequestService, _directMessageService, CurrentUserId);
                friendListViewModel.NavigateToProfileRequested += OnNavigateToProfileFromFriends;
                friendListViewModel.NavigateToChatRequested += OnNavigateToChatFromFriends;
                friendListViewModel.OpenRequestsRequested += OnOpenRequestsFromFriends;
                CurrentPage = friendListViewModel;
                return Task.CompletedTask;
            }

            CurrentPage = new FriendRequestsViewModel(_friendRequestService, CurrentUserId);
            return Task.CompletedTask;
        }

        private Task GoToProfileAsync()
            => ShowProfileAsync(CurrentUserId);


private async Task GoToEventsAsync()
{
    try 
    {
        // Use the repository that was passed into the constructor (this._eventRepository)
        // because that's the one we injected with the correct connection string!
        var vm = new EventListingViewModel(_eventRepository); 
        
        // Use the specific method name from their code
        await vm.LoadEventsAsync(); 
        
        CurrentPage = vm;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"NAVIGATION ERROR: {ex.Message}");
        // If it fails, we must set CurrentPage to something or it hangs
        CurrentPage = null; 
    }
}
private async Task GoToMyEventsAsync()
{
    // If they don't have a specific "MyEvents" method yet, 
    // this will at least show the public ones so the screen isn't blank.
    var vm = new EventListingViewModel(_eventRepository);
    await vm.LoadEventsAsync(); 
    
    CurrentPage = vm;
}

        private Task GoToReputationAsync()
        {
    // Matches: ReputationViewModel(IUserService, IReputationService)
            CurrentPage = new ReputationViewModel(_userService, _reputationService); 
            return Task.CompletedTask;
        }

        private Task GoToNotificationsAsync()
        {
            // Note: Check if NotificationsViewModel requires _notificationRepository
            CurrentPage = new NotificationViewModel(_notificationService, _userService); 
            return Task.CompletedTask;
        }


        private void OnNavigateToProfileFromFriends(Guid userId)
        {
            _ = ShowProfileAsync(userId);
        }

        private void OnNavigateToChatFromFriends(Guid conversationId)
        {
            NavigateToChatRequested?.Invoke(conversationId);
        }

        private void OnOpenRequestsFromFriends()
        {
            var friendRequestsViewModel = new FriendRequestsViewModel(_friendRequestService, CurrentUserId);
            friendRequestsViewModel.NavigateBackRequested += () => _ = GoToFriendsAsync();
            CurrentPage = friendRequestsViewModel;
        }

        private async Task ShowProfileAsync(Guid targetUserId)
        {
            var profileViewModel = new ProfileViewModel(_friendRequestService, _blockService, _directMessageService, _profileService, CurrentUserId);
            await profileViewModel.LoadAsync(targetUserId);
            CurrentPage = profileViewModel;
        }

        private Task LogoutAsync()
        {
            if (CurrentUserId != Guid.Empty)
            {
                _ = _profileService.UpdateStatusAsync(CurrentUserId, ChatModule.src.domain.Enums.UserStatus.Offline);
            }

            CurrentUserId = Guid.Empty;
            CurrentUsername = string.Empty;
            CurrentPage = null;
            NavigateToLoginRequested?.Invoke();
            return Task.CompletedTask;
        }
    }
}