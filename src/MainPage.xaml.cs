using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using KartProsjekt.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace KartProsjekt;

/// <summary>
/// The main content page displayed inside the application window.
/// Add your UI logic, event handlers, and data binding here.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();

        // TODO: Add your initialization logic here.
        // Select the default page
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardPage));
    }

    private void NavView_SelectionChanged(
       NavigationView sender,
       NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is not NavigationViewItem selectedItem)
            return;

        switch (selectedItem.Tag?.ToString())
        {
            case "Dashboard":
                ContentFrame.Navigate(typeof(DashboardPage));
                break;

                // Additional pages will be added here
        }
    }
}
