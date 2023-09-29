using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _7zip.ViewModels
{
    //注意：
    //1.解压时若调用同步Extract方法，只有将EventSynchronizationStrategy设为AlwaysSynchronous才触发FileExtractionStarted和FileExtractionFinished事件；
    //2.若调用异步Extract方法，只有设为将EventSynchronizationStrategy设为AlwaysAsynchronous才触发FileExtractionStarted和FileExtractionFinished事件。
    //3.SevenZipExtractor对象的创建线程和解压线程必须为同一个，否则抛出COM转换异常。

    internal partial class ExtractionViewModel : ObservableObject
    {
        #region Private Fields
        Type thisType;
        int[] filesIndexToExtract;
        List<ArchiveFileInfo> extractedFiles = new(10);
        bool shouldPause = false;
        bool shouldCancelWork = false;
        SemaphoreSlim pauseWorkSemaphore = new(1, 1);
        #endregion

        #region MVVM Properties
        /// <summary>
        /// 获取或设置要导出的压缩文件的路径。
        /// </summary>
        [ObservableProperty]
        string archivePath;

        /// <summary>
        /// 要导出的文件的索引集合。若为空，则解压所有文件。该集合不应有重复元素。
        /// </summary>
        [ObservableProperty]
        ObservableCollection<int> fileIndexes = new();

        /// <summary>
        /// 获取或设置解压的目标输出路径。
        /// </summary>
        [ObservableProperty]
        string outputDirPath;

        /// <summary>
        /// 获取或设置当前解压操作的总进度(0f-1f)。
        /// </summary>
        [ObservableProperty]
        float extractPercentage;

        /// <summary>
        /// 获取或设置当前正在解压的文件名。
        /// </summary>
        [ObservableProperty]
        string currentExtractingFileName;

        /// <summary>
        /// 获取或设置当前正在解压的文件索引。
        /// </summary>
        [ObservableProperty]
        int currentExtractingIndex;

        /// <summary>
        /// 获取或设置需要导出的文件总个数。
        /// </summary>
        [ObservableProperty]
        int totalFilesCount = -1;

        /// <summary>
        /// 获取或设置已导出的文件个数。
        /// </summary>
        [ObservableProperty]
        int extractedFilesCount = 0;


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPausing))]
        [NotifyPropertyChangedFor(nameof(IsPaused))]
        [NotifyPropertyChangedFor(nameof(IsFinished))]
        [NotifyPropertyChangedFor(nameof(IsCancelling))]
        [NotifyPropertyChangedFor(nameof(IsCancelled))]
        [NotifyPropertyChangedFor(nameof(IsExtracting))]
        ExtractionStatus extractionStatus;

        /// <summary>
        /// 获取或设置一个值，指示了当前解压操作是否正在暂停。
        /// </summary>
        public bool IsPausing => ExtractionStatus == ExtractionStatus.Pausing;

        /// <summary>
        /// 获取或设置一个值，指示了当前解压操作是否已暂停。
        /// </summary>
        public bool IsPaused => ExtractionStatus == ExtractionStatus.Paused;

        /// <summary>
        /// 获取或设置一个值，指示了正在取消（但还未完成取消）解压操作。
        /// </summary>
        public bool IsCancelling => ExtractionStatus == ExtractionStatus.Cancelling;

        /// <summary>
        /// 获取或设置一个值，指示了解压操作是否被取消。
        /// </summary>
        public bool IsCancelled => ExtractionStatus == ExtractionStatus.Cancelled;

        /// <summary>
        /// 获取或设置一个值，指示了解压操作是否完成。
        /// </summary>
        public bool IsFinished => ExtractionStatus == ExtractionStatus.Finished;

        public bool IsExtracting => ExtractionStatus == ExtractionStatus.Extracting;
        #endregion
        public ExtractionViewModel(string archivePath)
        {
            ArchivePath = archivePath;
        }

        public ExtractionViewModel()
        { }

        void EventStartupFor(SevenZipExtractor extractor)
        {
            extractor.Extracting += Extractor_Extracting;
            extractor.ExtractionFinished += Extractor_ExtractionFinished;
            extractor.FileExtractionStarted += Extractor_FileExtractionStarted;
            extractor.FileExtractionFinished += Extractor_FileExtractionFinished;
            this.PropertyChanged += ExtractViewModel_PropertyChanged;
        }

        private void Extractor_ExtractionFinished(object sender, EventArgs e)
        {
            //如果因取消而结束，应当删除已经导出的文件。
            if (ExtractionStatus == ExtractionStatus.Cancelled)
                foreach (var file in extractedFiles)
                {
                    string path = $"{OutputDirPath}\\{file.FileName}";
                    try
                    {
                        if (file.IsDirectory)
                        {

                            if (Directory.Exists(path))
                            {
                                Directory.Delete(path, true);
                            }
                        }
                        else
                        {
                            File.Delete(path);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[At {nameof(Extractor_ExtractionFinished)}]:{ex.Message}");
                        //待定异常处理
                    }
                }
        }

        private void Extractor_FileExtractionStarted(object sender, FileInfoEventArgs e)
        {
            e.Cancel = shouldCancelWork;
            if (shouldCancelWork)
            {
                SetPropertyFromUIThreadAsync(nameof(ExtractionStatus), ExtractionStatus.Cancelled);
            }

            SetPropertyFromUIThreadAsync(nameof(CurrentExtractingFileName), e.FileInfo.FileName);
            SetPropertyFromUIThreadAsync(nameof(CurrentExtractingIndex), e.FileInfo.Index);
        }

        private void Extractor_FileExtractionFinished(object sender, FileInfoEventArgs e)
        {
            extractedFiles.Add(e.FileInfo);
            SetPropertyFromUIThreadAsync(nameof(ExtractedFilesCount), ExtractedFilesCount + 1).Wait();

            if (ExtractedFilesCount == TotalFilesCount)
            {
                SetPropertyFromUIThreadAsync(nameof(ExtractionStatus), ExtractionStatus.Finished);
                return;
            }

            if (shouldPause)
            {
                SetPropertyFromUIThreadAsync(nameof(ExtractionStatus), ExtractionStatus.Paused);
                pauseWorkSemaphore.Wait();
                pauseWorkSemaphore.Release();
            }
        }

        private void Extractor_Extracting(object sender, ProgressEventArgs e)
        {
            SetPropertyFromUIThreadAsync(nameof(ExtractPercentage), e.PercentDone / 100f);
        }

        private void ExtractViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// 重置解压操作的所有统计数据，以便执行新的解压操作。
        /// </summary>
        void ResetExtractionStatistics()
        {
            extractedFiles.Clear();
            ExtractPercentage = 0;
            ExtractedFilesCount = 0; 
            TotalFilesCount = -1;
        }

        /// <summary>
        /// 通过反射从UI线程更新该ViewModel的属性。
        /// </summary>
        /// <param name="propertyName">属性名称，强烈建议使用nameof</param>
        /// <param name="newValue">要赋予的值</param>
        /// <param name="generateWaitTask">是否要生成该操作的Task，以便进行等待。</param>
        Task SetPropertyFromUIThreadAsync(string propertyName, object newValue)
        {
            thisType ??= this.GetType();
            var propertyInfo = thisType.GetProperty(propertyName, System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public);
            var task = App.MainDispatcherQueue.EnqueueAsync(() => propertyInfo.SetValue(this, newValue));
            return task;
        }


        /// <summary>
        /// 请求取消解压，解压操作将会在下个文件开始解压前完成取消。
        /// </summary>
        [RelayCommand]
        public void CancelWork()
        {
            shouldCancelWork = true;
            ExtractionStatus = ExtractionStatus.Cancelling;
            ResumeWork(); //取消解压时，需要使暂停的解压操作继续，来取消线程(信号量)堵塞，以执行取消操作。
        }

        [RelayCommand]
        public void PauseWork()
        {
            if (!shouldPause)
            {
                ExtractionStatus = ExtractionStatus.Pausing;
                pauseWorkSemaphore.Wait();
                shouldPause = true;
            }
        }

        [RelayCommand]
        public void ResumeWork()
        {
            if (shouldPause)
            {
                pauseWorkSemaphore.Release();
                shouldPause = false;
                if (!shouldCancelWork)
                    ExtractionStatus = ExtractionStatus.Extracting;
            }
        }

        [RelayCommand]
        public void TogglePause()
        {
            if (shouldPause)
                ResumeWork();
            else PauseWork();
        }

        void InnerExtract()
        {
            using var extractor = new SevenZipExtractor(ArchivePath);
            ResetExtractionStatistics();
            EventStartupFor(extractor);
            if (FileIndexes.Any())
            {
                filesIndexToExtract = FileIndexes.ToArray();
            }
            else
            {
                filesIndexToExtract = extractor.ArchiveFileData.Select(d => d.Index).ToArray();
            }

            SetPropertyFromUIThreadAsync(nameof(TotalFilesCount), filesIndexToExtract.Length);

            SetPropertyFromUIThreadAsync(nameof(ExtractionStatus), ExtractionStatus.Extracting);
            extractor.ExtractFiles(OutputDirPath, filesIndexToExtract);
        }

        [RelayCommand]
        public async Task ExtractAsync()
        {
            await Task.Run(InnerExtract);
        }
    }

    public enum ExtractionStatus
    {
        None = 0,
        Extracting,
        Finished,
        Pausing,
        Paused,
        Cancelling,
        Cancelled
    }
}
