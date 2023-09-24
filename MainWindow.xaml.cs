using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace _7zip;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow 
{
    public MainWindow()
    {
        InitializeComponent();
        this.CenterOnScreen();
        ExtendsContentIntoTitleBar = true;
        Title = AppWindow.Title;
        IsMaximizable = false;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow?.SetIcon("Assets\\AppLogo.ico");
    }


    private  void MyButton_Click(object sender, RoutedEventArgs e)
    {
        
    }
}