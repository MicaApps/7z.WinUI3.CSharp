using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using _7zip.ViewModels;
using _7zip.Views.Windows;
using SevenZip;

namespace _7zip;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    static string[] commandLine;
    /// <summary>
    /// 获取应用启动时的命令行字符串数组。
    /// 注：CommandLine[0]始终为应用主dll的完整路径，C++调用CreateProcess传入的参数从CommandLine[1]开始存储。
    /// </summary>
    public static string[] CommandLine => commandLine ??= Environment.GetCommandLineArgs();
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

        //这里用于测试时输出命令行字符串。
        //string commandLineOutputPath = "D:\\github\\7zipoutput.txt";
        //File.Delete(commandLineOutputPath);
        //foreach (var s in CommandLine) File.AppendAllText(commandLineOutputPath, s);
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Host = ConfigureServives();
        if (CommandLine.Length >=3 && CommandLine[1] == "-extract")
        {
            ExtractTest();
            return;
        }
        else if (CommandLine.Length >= 3 && CommandLine[1] == "-compress" && CommandLine[2] == "-7z")
        {
            Compress7zTest();
            return;
        }

        m_window = MainWindow.Instance;

        Frame rootFrame = MainWindow.Instance.EnsureWindowIsInitialized();
        rootFrame.Navigate(typeof(Views.Pages.MainPage), args);



        m_window.Activate();
    }

    private async void ExtractTest()
    {
        string targetArchivePath = CommandLine[2];
        string outputPath = Path.GetDirectoryName(targetArchivePath);
        var extractViewModel = new ExtractionViewModel(new ExtractionInfoViewModel[] { new ExtractionInfoViewModel(targetArchivePath,null) });
        extractViewModel.OutputDirPath = outputPath;
        var ui = new OperationWindow();
        (ui.Content as FrameworkElement).DataContext = extractViewModel;
        ui.Activate();
        await extractViewModel.ExtractAsync();
        Application.Current.Exit();
    }

    private async void Compress7zTest()
    {
        string targetArchivePath = CommandLine[3];
        string outputPath = Path.GetDirectoryName(targetArchivePath);
        var compressionViewModel = GetService<CompressionViewModel>();
        // compressionViewModel.TargetArchivePath = targetArchivePath;
        // compressionViewModel. = outputPath;
        // compressionViewModel.CompressionMethod = CompressionMethod.BZip2;

        //CompressionViewModel暂未完全实现
        var ui = new OperationWindow();
        (ui.Content as FrameworkElement).DataContext = compressionViewModel;
        ui.Activate();
        await compressionViewModel.CompressAsync();
        Application.Current.Exit();
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
}
