using KartProsjekt.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Globalization.NumberFormatting;

namespace KartProsjekt.Views;

public sealed partial class SyncfusionDataGridPage : Page
{
    public SyncfusionDataGridPage()
    {
        InitializeComponent();

        // A fixed Norwegian currency formatter keeps financial values in NOK regardless of the machine's locale.
        var nokFormatter = new CurrencyFormatter("NOK", new[] { "nb-NO" }, "NO");
        BudgetColumn.NumberFormatter = nokFormatter;
        ActualCostColumn.NumberFormatter = nokFormatter;

        Loaded += SyncfusionDataGridPage_Loaded;
    }

    // Receives the existing shared ViewModel from the main navigation host.
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as ProjectsViewModel
            ?? throw new InvalidOperationException("SyncfusionDataGridPage requires a ProjectsViewModel.");
    }

    // Loads the existing SQL-backed collection only when no other page has loaded it yet.
    private async void SyncfusionDataGridPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProjectsViewModel viewModel)
        {
            await viewModel.LoadProjectsAsync();
        }
    }
}
