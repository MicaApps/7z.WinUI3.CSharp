using _7zip.Core.Enums;
using _7zip.ViewModels;
using _7zip.Helpers;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Maps;

namespace _7zip.Models
{
    public class ShellExInterface
    {
        public void ExtractZip(string ZipPath)
        {
            // 创建ViewModel
            ExtractionViewModel viewModel = new ExtractionViewModel(ZipPath);
            viewModel.OutputDirPath = (new FileInfo(ZipPath)).Directory + Path.GetFileNameWithoutExtension(ZipPath);

            // 设置ViewModel
            var window = WindowManager.CreateWindow(WindowType.Extraction);
            (window.Content as FrameworkElement).DataContext = viewModel;
            window.Activate();

            // 开始解压
            viewModel.ExtractAsync();
        }
    }
}
