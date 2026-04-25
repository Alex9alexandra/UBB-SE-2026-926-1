using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Events_GSS.Data.Models;
using Events_GSS.ViewModels;

namespace Events_GSS.Views;

public sealed partial class EventListingPage : Page
{
    public EventListingViewModel ViewModel { get; }

    // Change the constructor to accept the ViewModel
    public EventListingPage(EventListingViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
        this.EventsListView.ItemsSource = ViewModel.Events;
    }
    

    // Keep their click handlers for now, but we will eventually need to 
    // change these to RelayCommands to be fully MVVM compliant!
    private void OnEventTapped(object sender, TappedRoutedEventArgs e) { /* ... */ }
    private void OnCreateEventClicked(object sender, RoutedEventArgs e) { /* ... */ }
}