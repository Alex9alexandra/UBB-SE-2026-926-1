using Events_GSS.Data.Repositories.achievementRepository;
using Events_GSS.Data.Services.achievementServices;
using Events_GSS.Data.Services.reputationService;

using Events_GSS.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Events_GSS.Views;

public sealed partial class ReputationPage : Page
{
    public ReputationViewModel ViewModel { get; private set; }

    public ReputationPage(ReputationViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        this.InitializeComponent();
        _ = LoadSafeAsync();
    }

    private async Task LoadSafeAsync()
    {
        try
        {
            await ViewModel.LoadAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadAsync failed: {ex}");
        }
    }
}
