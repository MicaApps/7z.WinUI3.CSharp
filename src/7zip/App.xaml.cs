using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel;

using _7zip.ViewModels;
using _7zip.Views.Windows;
using AppInstance = Microsoft.Windows.AppLifecycle.AppInstance;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;

namespace _7zip;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public static DispatcherQueue MainDispatcherQueue;
    public static ILogger Logger { get; private set; }

    public IHost Host;

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        EnsureEarlyApp();
    }

    private void EnsureEarlyApp()
    {
        // Configure exception handlers
        UnhandledException += App_UnhandledException;

        // Configure 7z.dll
        SevenZip.SevenZipBase.SetLibraryPath(Package.Current.InstalledPath + "\\Assets\\7z.dll");
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = MainWindow.Instance;
        Host = ConfigureServives();

        AppInstance appInstance = AppInstance.GetCurrent();
        if (appInstance != null) 
        {
            ExtendedActivationKind kind = appInstance.GetActivatedEventArgs().Kind;

            if (kind == ExtendedActivationKind.Launch)
            {
                Frame rootFrame = MainWindow.Instance.EnsureWindowIsInitialized();
                rootFrame.Navigate(typeof(Views.Pages.MainPage), args);
                m_window.Activate();
            }

            else if(kind == ExtendedActivationKind.CommandLineLaunch)
            {
                var notification = new AppNotificationBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", "9813")
                .AddText("Andrew sent you a picture")
                .AddText("Check this out, The Enchantments in Washington!")
                .BuildNotification();

                AppNotificationManager.Default.Show(notification);
            }
        }
        
        
    }

    private IHost ConfigureServives()
    {
        return Microsoft.Extensions.Hosting.Host.
            CreateDefaultBuilder().
            UseContentRoot(AppContext.BaseDirectory).
            ConfigureServices((context, services) =>
            {
                // Configure ViewModels.
                services.AddTransient<CompressionViewModel>();
                services.AddTransient<ExtractionViewModel>();
            }).Build();
    }

    private Window m_window;

    #region Exception Handlers
    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
    }
    #endregion

    #region Rich Activation

    #endregion
}
