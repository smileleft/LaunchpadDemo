using System.Windows;

namespace LaunchControl.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private void SelectPage(UIElement page, NavIcon active)
    {
        ConsolePage.Visibility = Visibility.Collapsed;
        PropellantPage.Visibility = Visibility.Collapsed;
        PressPurgePage.Visibility = Visibility.Collapsed;
        PowerPage.Visibility = Visibility.Collapsed;
        MechanicalPage.Visibility = Visibility.Collapsed;
        page.Visibility = Visibility.Visible;

        NavOverview.IsActive = false;
        NavReadiness.IsActive = false;
        NavSequencer.IsActive = false;
        NavPropellant.IsActive = false;
        NavPressPurge.IsActive = false;
        NavPower.IsActive = false;
        NavMechanical.IsActive = false;
        active.IsActive = true;
    }

    private void ShowConsole(object sender, RoutedEventArgs e) => SelectPage(ConsolePage, NavOverview);
    private void ShowPropellant(object sender, RoutedEventArgs e) => SelectPage(PropellantPage, NavPropellant);
    private void ShowPressPurge(object sender, RoutedEventArgs e) => SelectPage(PressPurgePage, NavPressPurge);
    private void ShowPower(object sender, RoutedEventArgs e) => SelectPage(PowerPage, NavPower);
    private void ShowMechanical(object sender, RoutedEventArgs e) => SelectPage(MechanicalPage, NavMechanical);
}
