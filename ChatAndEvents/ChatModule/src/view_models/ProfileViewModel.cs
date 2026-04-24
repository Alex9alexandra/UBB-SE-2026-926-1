using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatModule.Models;
using ChatModule.Services;
using ChatModule.src.domain.Enums;

namespace ChatModule.ViewModels;
public class ProfileViewModel : BaseViewModel
{
    private readonly FriendRequestService friendRequestService;
    private readonly BlockService blockService;
    private readonly IDirectMessageService directMessageService;
    private readonly Guid currentUserId;
    private readonly IProfileService profileService;
    private bool suppressStatusUpdate;

    private User? user;
    public User? User
    {
        get => user;
        set
        {
            if (Set(ref user, value))
            {
                OnPropertyChanged(nameof(StatusBadgeText));
            }
        }
    }

    private bool isBlocked;
    public bool IsBlocked
    {
        get => isBlocked;
        set => Set(ref isBlocked, value);
    }

    private bool isOwnProfile;
    public bool IsOwnProfile
    {
        get => isOwnProfile;
        set => Set(ref isOwnProfile, value);
    }

    public ObservableCollection<User> MutualFriends { get; } = new ();

    private bool areMutualFriendsVisible;
    public bool AreMutualFriendsVisible
    {
        get => areMutualFriendsVisible;
        set => Set(ref areMutualFriendsVisible, value);
    }

    public event Action<Guid>? NavigateToChatRequested;

    public RelayCommand SendFriendRequestCommand { get; }
    public RelayCommand BlockUserCommand { get; }
    public RelayCommand UnblockUserCommand { get; }
    public RelayCommand OpenDmCommand { get; }

    private UserStatus selectedStatus;
    public UserStatus SelectedStatus
    {
        get => selectedStatus;
        set
        {
            if (Set(ref selectedStatus, value))
            {
                OnPropertyChanged(nameof(StatusBadgeText));

                if (!suppressStatusUpdate && User != null && IsOwnProfile)
                {
                    _ = UpdateStatusAsync(value);
                }
            }
        }
    }

    public string StatusBadgeText => SelectedStatus.ToString();

    private string? editBio;
    public string? EditBio
    {
        get => editBio;
        set
        {
            if (Set(ref editBio, value))
            {
                OnPropertyChanged(nameof(DisplayBio));
            }
        }
    }

    public string DisplayBio => string.IsNullOrWhiteSpace(EditBio) ? "No bio yet" : EditBio!;

    private string? editAvatarUrl;
    public string? EditAvatarUrl
    {
        get => editAvatarUrl;
        set => Set(ref editAvatarUrl, value);
    }

    private DateTime? editBirthday;
    public DateTime? EditBirthday
    {
        get => editBirthday;
        set
        {
            if (Set(ref editBirthday, value))
            {
                OnPropertyChanged(nameof(EditBirthdayOffset));
                OnPropertyChanged(nameof(IsBirthdayToday));
                OnPropertyChanged(nameof(BirthdayText));
            }
        }
    }

    public string BirthdayText => EditBirthday.HasValue
        ? EditBirthday.Value.ToString("MMMM d, yyyy")
        : "Birthday not set";

    public int MutualFriendsCount => MutualFriends.Count;

    public string MutualFriendsLabel => $"Mutual Friends ({MutualFriendsCount})";

    public RelayCommand SaveProfileCommand { get; }
    public RelayCommand LoadMutualFriendsCommand { get; }
    public ObservableCollection<UserStatus> AvailableStatuses { get; } =
    [
        UserStatus.Online,
        UserStatus.Offline,
        UserStatus.Busy
    ];

    public DateTimeOffset EditBirthdayOffset
    {
        get => EditBirthday.HasValue
            ? new DateTimeOffset(EditBirthday.Value.Date)
            : DateTimeOffset.Now;
        set
        {
            EditBirthday = value.DateTime.Date;
            OnPropertyChanged(nameof(IsBirthdayToday));
        }
    }

    public bool IsBirthdayToday
    {
        get
        {
            if (!EditBirthday.HasValue)
            {
                return false;
            }

            var today = DateTime.Today;
            return EditBirthday.Value.Month == today.Month && EditBirthday.Value.Day == today.Day;
        }
    }

    public ProfileViewModel(
        FriendRequestService friendRequestService,
        BlockService blockService,
        IDirectMessageService directMessageService,
        IProfileService profileService,
        Guid currentUserId)
    {
        this.friendRequestService = friendRequestService ?? throw new ArgumentNullException(nameof(friendRequestService));
        this.blockService = blockService ?? throw new ArgumentNullException(nameof(blockService));
        this.directMessageService = directMessageService ?? throw new ArgumentNullException(nameof(directMessageService));
        this.currentUserId = currentUserId;
        this.profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));

        MutualFriends.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(MutualFriendsCount));
            OnPropertyChanged(nameof(MutualFriendsLabel));
        };

        SendFriendRequestCommand = new RelayCommand(SendFriendRequestAsync);
        BlockUserCommand = new RelayCommand(BlockUserAsync);
        UnblockUserCommand = new RelayCommand(UnblockUserAsync);
        OpenDmCommand = new RelayCommand(OpenDmAsync);

        SaveProfileCommand = new RelayCommand(SaveProfileAsync);
        LoadMutualFriendsCommand = new RelayCommand(LoadMutualFriendsAsync);
    }

    public async Task LoadAsync(Guid targetUserId)
    {
        User = await profileService.GetProfileAsync(targetUserId);
        IsOwnProfile = targetUserId == currentUserId;

        suppressStatusUpdate = true;

        try
        {
            if (User != null)
            {
                SelectedStatus = User.Status;
                EditBio = User.Bio;
                EditAvatarUrl = User.AvatarUrl;
                EditBirthday = User.Birthday;
            }
        }
        finally
        {
            suppressStatusUpdate = false;
        }

        AreMutualFriendsVisible = false;

        if (!IsOwnProfile && User != null)
        {
            IsBlocked = await blockService.IsBlockedAsync(currentUserId, User.Id);
            await RefreshMutualFriendsAsync();
        }
        else
        {
            IsBlocked = false;
            MutualFriends.Clear();
        }
    }

    private async Task UpdateStatusAsync(UserStatus status)
    {
        if (User == null)
        {
            return;
        }

        await profileService.UpdateStatusAsync(User.Id, status);
    }

    private async Task SaveProfileAsync()
    {
        if (User == null)
        {
            return;
        }

        await profileService.UpdateProfileAsync(User.Id, EditBio, EditAvatarUrl, EditBirthday);
        User = await profileService.GetProfileAsync(User.Id);

        if (User != null)
        {
            suppressStatusUpdate = true;
            try
            {
                SelectedStatus = User.Status;
                EditBio = User.Bio;
                EditAvatarUrl = User.AvatarUrl;
                EditBirthday = User.Birthday;
            }
            finally
            {
                suppressStatusUpdate = false;
            }
        }
    }

    private async Task LoadMutualFriendsAsync()
    {
        if (IsOwnProfile || User == null)
        {
            return;
        }

        if (!AreMutualFriendsVisible || MutualFriends.Count == 0)
        {
            await RefreshMutualFriendsAsync();
        }

        AreMutualFriendsVisible = !AreMutualFriendsVisible;
    }

    private async Task RefreshMutualFriendsAsync()
    {
        MutualFriends.Clear();
        if (IsOwnProfile || User == null)
        {
            return;
        }

        var mutuals = await profileService.GetMutualFriendsAsync(currentUserId, User.Id);
        foreach (var user in mutuals)
        {
            MutualFriends.Add(user);
        }
    }

    private async Task SendFriendRequestAsync()
    {
        if (User == null)
        {
            return;
        }

        if (IsBlocked)
        {
            return;
        }

        var existingRelationshipStatus = await friendRequestService.GetRelationshipStatusAsync(currentUserId, User.Id);
        if (existingRelationshipStatus.HasValue)
        {
            return;
        }

        await friendRequestService.SendFriendRequestAsync(currentUserId, User.Id);
    }

    private async Task BlockUserAsync()
    {
        if (User == null)
        {
            return;
        }

        await blockService.BlockUserAsync(currentUserId, User.Id);
        IsBlocked = true;
    }

    private async Task UnblockUserAsync()
    {
        if (User == null)
        {
            return;
        }

        await blockService.UnblockUserAsync(currentUserId, User.Id);
        IsBlocked = false;
    }

    private async Task OpenDmAsync()
    {
        if (User == null)
        {
            return;
        }

        var conversation = await directMessageService.GetOrCreateAsync(currentUserId, User.Id);
        NavigateToChatRequested?.Invoke(conversation.Id);
    }
}
