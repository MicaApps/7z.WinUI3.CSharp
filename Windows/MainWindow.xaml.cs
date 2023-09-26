using _7zip.Windows;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinUIEx;


namespace _7zip;

public sealed partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        this.CenterOnScreen();
        this.SetIcon("Assets\\AppIcon.ico");
        ExtendsContentIntoTitleBar = true;
        Title = AppWindow.Title;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        new OperationWindow().Activate();
    }
}