using System.Windows;

namespace LaunchControl.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ShowConsole(object sender, RoutedEventArgs e)
    {
        ConsolePage.Visibility = Visibility.Visible;
        PropellantPage.Visibility = Visibility.Collapsed;
        PressPurgePage.Visibility = Visibility.Collapsed;
        NavOverview.IsActive = true;
        NavReadiness.IsActive = false;
        NavSequencer.IsActive = false;
        NavPropellant.IsActive = false;
        NavPressPurge.IsActive = false;
    }

    private void ShowPropellant(object sender, RoutedEventArgs e)
    {
        ConsolePage.Visibility = Visibility.Collapsed;
        PropellantPage.Visibility = Visibility.Visible;
        PressPurgePage.Visibility = Visibility.Collapsed;
        NavOverview.IsActive = false;
        NavReadiness.IsActive = false;
        NavSequencer.IsActive = false;
        NavPropellant.IsActive = true;
        NavPressPurge.IsActive = false;
    }

    private void ShowPressPurge(object sender, RoutedEventArgs e)
    {
        ConsolePage.Visibility = Visibility.Collapsed;
        PropellantPage.Visibility = Visibility.Collapsed;
        PressPurgePage.Visibility = Visibility.Visible;
        NavOverview.IsActive = false;
        NavReadiness.IsActive = false;
        NavSequencer.IsActive = false;
        NavPropellant.IsActive = false;
        NavPressPurge.IsActive = true;
    }
}
