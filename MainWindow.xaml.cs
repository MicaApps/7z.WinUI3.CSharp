using System;
using Windows.Foundation;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinUIEx;
using System.Runtime.InteropServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace _7zip;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow 
{
    // µ¼Èë Win32 API º¯Êý
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool InsertMenu(IntPtr hMenu, uint uPosition, uint uFlags, uint uIDNewItem, string lpNewItem);
    public MainWindow()
    {
        InitializeComponent();
        this.CenterOnScreen();
        ExtendsContentIntoTitleBar = true;
        Title = AppWindow.Title;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow?.SetIcon("Assets\\AppLogo.ico");
    }



    
}