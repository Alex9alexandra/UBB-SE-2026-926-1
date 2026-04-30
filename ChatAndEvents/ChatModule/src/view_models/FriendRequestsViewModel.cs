using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatModule.src.Interfaces.Services;
using ChatModule.ViewModels;

namespace ChatModule.src.view_models
{
    public class FriendRequestsViewModel : BaseViewModel
    {
        private readonly IFriendRequestService friendRequestService;
        private readonly Guid currentUserId;

        public ObservableCollection<User> IncomingRequests { get; } = new ();

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => Set(ref isLoading, value);
        }

        public event Action? NavigateBackRequested;

        public RelayCommand LoadCommand { get; }
        public RelayCommand<Guid> AcceptCommand { get; }
        public RelayCommand<Guid> DeclineCommand { get; }
        public RelayCommand BackCommand { get; }

        public FriendRequestsViewModel(IFriendRequestService friendRequestService, Guid currentUserId)
        {
            this.friendRequestService = friendRequestService ?? throw new ArgumentNullException(nameof(friendRequestService));
            this.currentUserId = currentUserId;

            LoadCommand = new RelayCommand(LoadIncomingRequestsAsync);
            AcceptCommand = new RelayCommand<Guid>(AcceptFriendRequestAsync);
            DeclineCommand = new RelayCommand<Guid>(DeclineFriendRequestAsync);
            BackCommand = new RelayCommand(NavigateBackAsync);
        }

        private async Task LoadIncomingRequestsAsync()
        {
            IsLoading = true;
            try
            {
                var pendingRequestsList = await friendRequestService.GetIncomingRequestsAsync(currentUserId);
                IncomingRequests.Clear();
                foreach (var requesterUser in pendingRequestsList)
                {
                    IncomingRequests.Add(requesterUser);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AcceptFriendRequestAsync(Guid requesterUserId)
        {
            await friendRequestService.AcceptFriendRequestAsync(currentUserId, requesterUserId);
            await LoadIncomingRequestsAsync();
        }

        private async Task DeclineFriendRequestAsync(Guid requesterUserId)
        {
            await friendRequestService.DeclineFriendRequestAsync(currentUserId, requesterUserId);
            await LoadIncomingRequestsAsync();
        }

        private Task NavigateBackAsync()
        {
            NavigateBackRequested?.Invoke();
            return Task.CompletedTask;
        }
    }
}
