using _7zip.Helpers;
using _7zip.ViewModels;
using _7zip.Views.Windows;
using _7zip.Core.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace _7zip.Views.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        //创建ViewModel
        ExtractionViewModel viewModel = new ExtractionViewModel(archivePathTxtBox.Text);
        viewModel.OutputDirPath = outputPathTxtBox.Text;

        //设置ViewModel
        var window = WindowManager.CreateWindow(WindowType.Extraction);
        (window.Content as FrameworkElement).DataContext = viewModel;
        window.Activate();

        //开始解压
        viewModel.ExtractAsync();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var window = WindowManager.CreateWindow(WindowType.Compression);
        window.Activate();
    }
}
