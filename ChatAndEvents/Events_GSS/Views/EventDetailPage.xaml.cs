using System;
using System.Threading.Tasks;
using Events_GSS.Data.Models;
using Events_GSS.Data.Services.announcementServices;
using Events_GSS.Data.Services.discussionService;
using Events_GSS.Data.Services.Interfaces;
using Events_GSS.Services.Interfaces;
using Events_GSS.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Events_GSS.Views;

public sealed partial class EventDetailPage : Page
{
    // The Wrapper ViewModel we created
    public EventDetailViewModel ViewModel { get; }

    private Event currentEvent;
    private IAttendedEventService? attendedEventService;
    private bool isEnrolled;

    public EventDetailPage(EventDetailViewModel viewModel)
    {
        this.InitializeComponent();
        
        this.ViewModel = viewModel;
        this.currentEvent = viewModel.SelectedEvent;

        // Bypass OnNavigatedTo and load the tabs immediately
        SetupPageData();
    }

    private void SetupPageData()
    {
        this.EventNameText.Text = currentEvent.Name;
        this.EventInfoText.Text = $"{currentEvent.StartDateTime:MMM dd, yyyy HH:mm} • {currentEvent.Location}";

        var userService = App.Services.GetRequiredService<IUserService>();
        var currentUser = userService.GetCurrentUser();
        Guid userId = currentUser.UserId;
        bool isAdmin = currentEvent.Admin?.UserId == userId;

        if (isAdmin)
        {
            this.StatisticsButton.Visibility = Visibility.Visible;
        }

        var announcementService = App.Services.GetRequiredService<IAnnouncementService>();
        var announcementViewModel = new AnnouncementViewModel(currentEvent, announcementService, userId, isAdmin);
        this.AnnouncementTab.ViewModel = announcementViewModel;
        _ = announcementViewModel.InitializeAsync();

        var discussionService = App.Services.GetRequiredService<IDiscussionService>();
        var discussionViewModel = new DiscussionViewModel(currentEvent, discussionService, userId, isAdmin);
        this.DiscussionTab.ViewModel = discussionViewModel;
        _ = discussionViewModel.InitializeAsync();

        this.QuestAdminTab.ViewModel = new QuestApprovalViewModel(new QuestAdminViewModel(currentEvent));
        this.QuestUserTab.ViewModel = new QuestUserViewModel(currentEvent);
        if (isAdmin)
        {
            this.QuestAdminTab.Visibility = Visibility.Visible;
            this.QuestUserTab.Visibility = Visibility.Collapsed;
        }

        var memoryService = App.Services.GetRequiredService<IMemoryService>();
        var memoryViewModel = new MemoryViewModel(memoryService);
        this.MemoryTab.ViewModel = memoryViewModel;
        _ = memoryViewModel.InitializeAsync(currentEvent, currentUser);

        this.attendedEventService = App.Services.GetRequiredService<IAttendedEventService>();
        
        // Use the ! to guarantee to the compiler that currentEvent is not null
        _ = this.LoadEnrollmentStatusAsync(currentEvent!, userId);
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)
    {
        // Tell the MainViewModel to swap the page back to the list
        ViewModel.RequestBack();
    }

    private void OnStatisticsClicked(object sender, RoutedEventArgs e)
    {
        // Their old NavigationService is gone, so for now we just print to debug.
        // To fix this later, you add a 'RequestStatistics(currentEvent)' shout to the ViewModel!
        System.Diagnostics.Debug.WriteLine("Statistics button clicked!");
    }

    // --- THEIR ORIGINAL UI LOGIC REMAINS INTACT BELOW ---

    private async Task LoadEnrollmentStatusAsync(Event ev, Guid userId)
    {
        var attendedEvent = await this.attendedEventService!.GetAsync(ev.EventId, userId);
        this.isEnrolled = attendedEvent != null;
        this.JoinLeaveButton.Content = this.isEnrolled ? "Leave Event" : "Join Event";
    }

    private async void OnJoinLeaveClicked(object sender, RoutedEventArgs e)
    {
        if (this.currentEvent == null) return;

        var userService = App.Services.GetRequiredService<IUserService>();
        var userId = userService.GetCurrentUser().UserId;

        try
        {
            this.JoinLeaveButton.IsEnabled = false;

            if (this.isEnrolled)
            {
                await this.attendedEventService!.LeaveEventAsync(this.currentEvent.EventId, userId);
                this.isEnrolled = false;
                this.JoinLeaveButton.Content = "Join Event";
            }
            else
            {
                await this.attendedEventService!.AttendEventAsync(this.currentEvent.EventId, userId);
                this.isEnrolled = true;
                this.JoinLeaveButton.Content = "Leave Event";
            }
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Join/Leave failed: {exception.Message}");
        }
        finally
        {
            this.JoinLeaveButton.IsEnabled = true;
        }
    }
}