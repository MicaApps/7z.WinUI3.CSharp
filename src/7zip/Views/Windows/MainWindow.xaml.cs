using _7zip.ViewModels;
using _7zip.Views.Windows;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using Windows.ApplicationModel;
using WinUIEx;


namespace _7zip.Views.Windows;

public sealed partial class MainWindow : WindowEx
{

    private static MainWindow _instance;

    public static MainWindow Instance => _instance ?? (_instance = new MainWindow());

    public MainWindow()
    {
        InitializeComponent();

        EnsureEarlyWindow();
    }

    private void EnsureEarlyWindow()
    {
        App.MainDispatcherQueue = this.DispatcherQueue;
        this.CenterOnScreen();
        this.SetIcon("Assets\\AppIcon.ico");

        MinHeight = 416;
        MinWidth = 516;

        AppWindow.Title = "7 Zip";
        AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "/Assets/AppIcon.ico"));
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
    }

    public Frame EnsureWindowIsInitialized()
    {
        // NOTE:
        //  Do not repeat app initialization when the Window already has content,
        //  just ensure that the window is active
        if (Instance.WindowContent is not Frame rootFrame)
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = new() { CacheSize = 1 };

            // Place the frame in the current Window
            Instance.WindowContent = rootFrame;
        }

        return rootFrame;
    }
}