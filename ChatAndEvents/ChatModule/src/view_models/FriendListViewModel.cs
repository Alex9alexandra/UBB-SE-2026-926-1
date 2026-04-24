using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatModule.Models;
using ChatModule.Services;
using ChatModule.src.Interfaces.Services;
using ChatModule.ViewModels;

namespace ChatModule.src.view_models
{
    public class FriendListViewModel : BaseViewModel
    {
        private readonly IFriendListService friendListService;
        private readonly IFriendRequestService friendRequestService;
        private IDirectMessageService? directMessageService;
        private readonly Guid currentUserId;

        public ObservableCollection<User> Friends { get; } = new ();
        public ObservableCollection<FriendListItemViewModel> FriendItemViewModels { get; } = new ();

        public bool HasFriends => FriendItemViewModels.Count > 0;
        public bool ShowFriendList => !IsLoading && HasFriends;
        public bool ShowEmptyState => !IsLoading && !HasFriends;

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (Set(ref isLoading, value))
                {
                    OnPropertyChanged(nameof(ShowFriendList));
                    OnPropertyChanged(nameof(ShowEmptyState));
                }
            }
        }

        public event Action<Guid>? NavigateToChatRequested;
        public event Action<Guid>? NavigateToProfileRequested;
        public event Action? OpenRequestsRequested;

        public RelayCommand LoadCommand { get; }
        public RelayCommand<Guid> OpenDirectMessageCommand { get; }
        public RelayCommand<Guid> ViewProfileCommand { get; }
        public RelayCommand<Guid> RemoveFriendCommand { get; }
        public RelayCommand SendFriendRequestCommand { get; }
        public RelayCommand OpenRequestsCommand { get; }

        private string friendUsernameInput = string.Empty;
        public string FriendUsernameInput
        {
            get => friendUsernameInput;
            set => Set(ref friendUsernameInput, value);
        }

        private string? friendActionMessage;
        public string? FriendActionMessage
        {
            get => friendActionMessage;
            set => Set(ref friendActionMessage, value);
        }

        public FriendListViewModel(
            IFriendListService friendListService,
            IFriendRequestService friendRequestService,
            Guid currentUserId)
        {
            this.friendListService = friendListService ?? throw new ArgumentNullException(nameof(friendListService));
            this.friendRequestService = friendRequestService ?? throw new ArgumentNullException(nameof(friendRequestService));
            this.currentUserId = currentUserId;

            FriendItemViewModels.CollectionChanged += (sender, eventArgs) =>
            {
                OnPropertyChanged(nameof(HasFriends));
                OnPropertyChanged(nameof(ShowFriendList));
                OnPropertyChanged(nameof(ShowEmptyState));
            };

            LoadCommand = new RelayCommand(LoadFriendsAsync);
            OpenDirectMessageCommand = new RelayCommand<Guid>(OpenDirectMessageAsync);
            ViewProfileCommand = new RelayCommand<Guid>(ViewProfileAsync);
            RemoveFriendCommand = new RelayCommand<Guid>(RemoveFriendAsync);
            SendFriendRequestCommand = new RelayCommand(SendFriendRequestAsync);
            OpenRequestsCommand = new RelayCommand(OpenRequestsAsync);
        }

        public FriendListViewModel(
            IFriendListService friendListService,
            IFriendRequestService friendRequestService,
            IDirectMessageService directMessageService,
            Guid currentUserId)
            : this(friendListService, friendRequestService, currentUserId)
        {
            this.directMessageService = directMessageService ?? throw new ArgumentNullException(nameof(directMessageService));
        }

        public void SetDirectMessageService(IDirectMessageService directMessageService)
        {
            this.directMessageService = directMessageService ?? throw new ArgumentNullException(nameof(directMessageService));
        }

        private async Task LoadFriendsAsync()
        {
            IsLoading = true;
            try
            {
                var friendsList = await friendListService.GetFriendsAsync(currentUserId);
                Friends.Clear();
                FriendItemViewModels.Clear();
                foreach (var friend in friendsList)
                {
                    Friends.Add(friend);
                    FriendItemViewModels.Add(new FriendListItemViewModel(friend));
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OpenDirectMessageAsync(Guid targetUserId)
        {
            if (targetUserId == Guid.Empty)
            {
                return;
            }

            if (directMessageService == null)
            {
                return;
            }

            var activeConversation = await directMessageService.GetOrCreateAsync(currentUserId, targetUserId);
            NavigateToChatRequested?.Invoke(activeConversation.Id);
        }

        private Task ViewProfileAsync(Guid targetUserId)
        {
            if (targetUserId == Guid.Empty)
            {
                return Task.CompletedTask;
            }

            NavigateToProfileRequested?.Invoke(targetUserId);
            return Task.CompletedTask;
        }

        private async Task RemoveFriendAsync(Guid targetUserId)
        {
            if (targetUserId == Guid.Empty)
            {
                return;
            }

            await friendListService.RemoveFriendAsync(currentUserId, targetUserId);
            await LoadFriendsAsync();
        }

        private async Task SendFriendRequestAsync()
        {
            FriendActionMessage = null;

            if (string.IsNullOrWhiteSpace(FriendUsernameInput))
            {
                FriendActionMessage = "Enter a username first.";
                return;
            }

            try
            {
                var isRequestSent = await friendRequestService.SendFriendRequestByUsernameAsync(currentUserId, FriendUsernameInput);
                if (!isRequestSent)
                {
                    FriendActionMessage = "User not found.";
                    return;
                }

                FriendActionMessage = "Friend request sent.";
                FriendUsernameInput = string.Empty;
            }
            catch (Exception exception)
            {
                FriendActionMessage = exception.Message;
            }
        }

        private Task OpenRequestsAsync()
        {
            OpenRequestsRequested?.Invoke();
            return Task.CompletedTask;
        }
    }
}
