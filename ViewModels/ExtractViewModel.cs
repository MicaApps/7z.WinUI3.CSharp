using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7zip.ViewModels
{
    internal partial class ExtractionViewModel : ObservableObject
    {
        SevenZipExtractor extractor;

        /// <summary>
        /// 获取压缩文件的路径。
        /// </summary>
        public string ArchivePath => extractor.FileName;

        /// <summary>
        /// 要导出的文件的索引集合。若为空，则应解压所有文件。
        /// </summary>
        [ObservableProperty]
        ObservableCollection<int> fileIndexes = new();

        /// <summary>
        /// 获取或设置解压的目标输出路径。
        /// </summary>
        [ObservableProperty]
        string outputDirPath;

        /// <summary>
        /// 获取当前解压操作的总进度(0f-1f)。
        /// </summary>
        [ObservableProperty]
        float extractPercentage;

        /// <summary>
        /// 获取当前正在解压的文件名。
        /// </summary>
        [ObservableProperty]
        string currentExtractingFileName;

        /// <summary>
        /// 获取当前正在解压的文件索引。
        /// </summary>
        [ObservableProperty]
        int currentExtractingIndex;

        public ExtractionViewModel(string archivePath)
        {
            extractor = new SevenZipExtractor(archivePath)
            //必须使事件调用异步，否则不触发FileExtractionStarted和FileExtractionFinished事件。
            { EventSynchronization = EventSynchronizationStrategy.AlwaysAsynchronous }; 

            EventStartup();
        }

        void EventStartup()
        {
            extractor.Extracting += Extractor_Extracting;
            extractor.FileExtractionStarted += Extractor_FileExtractionStarted;
            extractor.FileExtractionFinished += Extractor_FileExtractionFinished;
            this.PropertyChanged += ExtractViewModel_PropertyChanged;
        }

        private void Extractor_FileExtractionStarted(object sender, FileInfoEventArgs e)
        {
            CurrentExtractingFileName = e.FileInfo.FileName;
            CurrentExtractingIndex = e.FileInfo.Index;
        }

        private void Extractor_FileExtractionFinished(object sender, FileInfoEventArgs e)
        {
            
        }

        private void Extractor_Extracting(object sender, ProgressEventArgs e)
        {
            ExtractPercentage = e.PercentDone / 100f;
        }

        private void ExtractViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        [RelayCommand]
        public async Task ExtractAsync()
        {
            if (FileIndexes.Any())
            {
                await extractor.ExtractFilesAsync(OutputDirPath, FileIndexes.ToArray());
            }
            else
            {
                await extractor.ExtractArchiveAsync(OutputDirPath);
            }
        }
    }
}
