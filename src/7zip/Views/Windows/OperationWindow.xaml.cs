using Microsoft.UI.Xaml;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace _7zip.Views.Windows;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class OperationWindow
{
    public OperationWindow()
    {
        InitializeComponent();

        App.MainDispatcherQueue = this.DispatcherQueue;
        this.CenterOnScreen();
        ExtendsContentIntoTitleBar = true;
        Title = AppWindow.Title;
        var manager = WindowManager.Get(this);
        this.SetIcon("Assets\\AppIcon.ico");
        manager.IsResizable = false;
        this.SetWindowStyle(WindowStyle.MinimizeBox | WindowStyle.Caption);
    }
}
