using ChatModule.Services;
using ChatModule.src.view_models;
using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories.eventRepository;
using Events_GSS.Data.Repositories.notificationRepository;
using Events_GSS.Data.Repositories.reputationRepository;
using Events_GSS.Data.Services.eventServices;
using Events_GSS.Data.Services.Interfaces; // For IEventService, IQuestService, IAttendedEventService
using Events_GSS.Data.Services.notificationServices;
using Events_GSS.Data.Services.reputationService;
using Events_GSS.Services.Interfaces; // For IUserService
using Events_GSS.ViewModels;
using System;
using System.Threading.Tasks;
using Events_GSS.Views; 

namespace ChatModule.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // --- Chat Services ---
        private readonly ConversationListService _conversationListService;
        private readonly FriendRequestService _friendRequestService;
        private readonly FriendListService? _friendListService;
        private readonly BlockService _blockService;
        private readonly IDirectMessageService _directMessageService;
        private readonly IProfileService _profileService;

        // --- Event/GSS Services ---
        private readonly IEventRepository _eventRepository;
        private readonly INotificationService _notificationService;
        private readonly IReputationService _reputationService;
        private readonly IUserService _userService;

        // Needed specifically for creating an event
        private readonly IEventService _eventService;
        private readonly IQuestService _questService;
        private readonly IAttendedEventService _attendedEventService;

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
        public RelayCommand GoToCreateEventCommand { get; }

        // Constructor 1 (Without FriendListService)
        public MainViewModel(
            ConversationListService conversationListService,
            FriendRequestService friendRequestService,
            BlockService blockService,
            IProfileService profileService,
            IDirectMessageService directMessageService,
            // --- GSS SERVICES ---
            IEventRepository eventRepository,
            INotificationService notificationService,
            IReputationService reputationService,
            IUserService userService,
            IEventService eventService,
            IQuestService questService,
            IAttendedEventService attendedEventService)
            : this(
                conversationListService,
                friendRequestService,
                null, // friendListService
                blockService,
                profileService,
                directMessageService,
                eventRepository,
                notificationService,
                reputationService,
                userService,
                eventService,
                questService,
                attendedEventService)
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
            // --- GSS SERVICES ---
            IEventRepository eventRepository,
            INotificationService notificationService,
            IReputationService reputationService,
            IUserService userService,
            IEventService eventService,
            IQuestService questService,
            IAttendedEventService attendedEventService)
        {
            _conversationListService = conversationListService ?? throw new ArgumentNullException(nameof(conversationListService));
            _friendRequestService = friendRequestService ?? throw new ArgumentNullException(nameof(friendRequestService));
            _friendListService = friendListService;
            _blockService = blockService ?? throw new ArgumentNullException(nameof(blockService));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _directMessageService = directMessageService ?? throw new ArgumentNullException(nameof(directMessageService));

            // Assign the Events/GSS services
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _reputationService = reputationService ?? throw new ArgumentNullException(nameof(reputationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _questService = questService ?? throw new ArgumentNullException(nameof(questService));
            _attendedEventService = attendedEventService ?? throw new ArgumentNullException(nameof(attendedEventService));

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
            GoToCreateEventCommand = new RelayCommand(GoToCreateEventAsync);
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
            var vm = new EventListingViewModel(_eventRepository);

            // --- THIS IS YOUR MISSING CODE ---
            // Listen for the "Create" shout, and execute the existing command if allowed
            vm.CreateEventRequested += () =>
            {
                if (GoToCreateEventCommand.CanExecute(null))
                {
                    GoToCreateEventCommand.Execute(null);
                }
            };
            // ---------------------------------

            // This is the "Listener" that connects the two.
            // Visual Studio's static analysis sometimes misses this as a 'reference'.
            vm.EventDetailsRequested += (selectedEvent) =>
            {
                _ = GoToEventDetailsAsync(selectedEvent); // <--- Call it here!
            };

            await vm.LoadEventsAsync();
            CurrentPage = vm;
        }

        /// <summary>
        /// Aici e doar sa arate toate, trebuie facut sa le poti vedea doar pe ale tale
        /// </summary>
        /// <returns></returns>
        private async Task GoToMyEventsAsync()
        {
            var vm = new MyEventsViewModel(_eventService, _userService, _attendedEventService);

            vm.EventDetailsRequested += (selectedEvent) =>
            {
                _ = GoToMyEventDetailsAsync(selectedEvent);
            };

            await vm.LoadMyEventsAsync();
            CurrentPage = vm;
        }

        private Task GoToMyEventDetailsAsync(Event selectedEvent)
        {
            var vm = new EventDetailViewModel(selectedEvent, this._attendedEventService);

            // Back from detail goes back to My Events, not the public list
            vm.BackRequested += () => _ = this.GoToMyEventsAsync();
            this.CurrentPage = vm;
            return Task.CompletedTask;
        }

        private async Task GoToReputationAsync()
        {
            var vm = new ReputationViewModel(_userService, _reputationService);
            await vm.LoadAsync();
            CurrentPage = vm;
        }

        private async Task GoToNotificationsAsync()
        {
            var vm = new NotificationViewModel(_notificationService, _userService);
            await vm.LoadAsync();

            CurrentPage = vm;
        }

        private async Task GoToCreateEventAsync()
        {
            try
            {
                // 1. Create the ViewModel with all required services
                var vm = new CreateEventViewModel(
                    _userService,
                    _eventService,
                    _questService,
                    _attendedEventService);

                // 2. Load the preset quests so Step 2/3 isn't empty
                await vm.LoadPresetQuestsAsync();

                // 3. Set up the event handler for when the user clicks "Cancel" or finishes creating
                vm.CloseRequested += (createdEventDto) =>
                {
                    // Whether they saved or canceled, take them back to the Events list
                    _ = GoToEventsAsync();
                };

                // 4. Show the page
                CurrentPage = vm;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Create Event page: {ex.Message}");
            }
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

        private Task GoToEventDetailsAsync(Event selectedEvent)
        {
            // Pass the service we already have in MainViewModel
            var vm = new EventDetailViewModel(selectedEvent, _attendedEventService);

            vm.BackRequested += () => _ = GoToEventsAsync();
            CurrentPage = vm;
            return Task.CompletedTask;
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