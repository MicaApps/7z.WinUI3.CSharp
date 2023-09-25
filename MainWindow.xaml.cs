using Microsoft.UI.Windowing;
using WinUIEx;


namespace _7zip;

public sealed partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        this.CenterOnScreen();
        ExtendsContentIntoTitleBar = true;
        Title = AppWindow.Title;
        var manager = WindowManager.Get(this);
        this.SetIcon("Assets\\AppIcon.ico");
        manager.IsMaximizable = false;
        manager.IsResizable = false;
        // this.SetWindowStyle(WindowStyle.OverlappedWindow);
    }
}