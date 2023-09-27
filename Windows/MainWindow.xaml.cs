using _7zip.ViewModels;
using _7zip.Windows;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.IO;
using WinUIEx;


namespace _7zip;

public sealed partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        App.MainDispatcherQueue = this.DispatcherQueue;
        this.CenterOnScreen();
        this.SetIcon("Assets\\AppIcon.ico");
        ExtendsContentIntoTitleBar = true;
        Title = AppWindow.Title;
       
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        //创建ViewModel
        ExtractionViewModel viewModel = new ExtractionViewModel(archivePathTxtBox.Text);
        viewModel.OutputDirPath = outputPathTxtBox.Text;

        //设置ViewModel
        var window = new OperationWindow();
        (window.Content as FrameworkElement).DataContext = viewModel;
        window.Activate();

        //开始解压
        viewModel.ExtractAsync();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var window = new NewCompressionWindow();
        window.Activate();
    }
}