using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.VisualBasic.FileIO;
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
using Windows.Storage;

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
        bool pauseRequested = false;
        bool cancelRequested = false;
        string tempOutputDir;
        SemaphoreSlim pauseWorkSemaphore = new(1, 1);
        List<ArchiveFileInfo> extractedFiles = new(10);
        #endregion

        #region MVVM Properties
        /// <summary>
        /// 获取要导出的压缩文件的路径。
        /// </summary>
        public List<ExtractionInfo> ExtractionInfos { get; private set; }

        /// <summary>
        /// 要导出的文件的索引集合。若为空，则解压所有文件。该集合不应有重复元素。
        /// </summary>
        [ObservableProperty]
        ObservableCollection<int> fileIndexes = new();

        /// <summary>
        /// 获取或设置解压使用的中转缓存目录位置。
        /// </summary>
        [ObservableProperty]
        ExtractionTempOutputPosition tempOutputPosition;

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
        public ExtractionViewModel(IEnumerable<ExtractionInfo> extractionInfos)
        {
            ExtractionInfos = extractionInfos.ToList() ?? throw new ArgumentNullException(nameof(extractionInfos));
            this.PropertyChanged += ExtractViewModel_PropertyChanged;
        }

        /// <summary>
        /// 为指定的<see cref="SevenZipExtractor"/>注册事件。
        /// </summary>
        /// <param name="extractor"></param>
        void EventStartupFor(SevenZipExtractor extractor)
        {
            extractor.Extracting += Extractor_Extracting;
            extractor.ExtractionFinished += Extractor_ExtractionFinished;
            extractor.FileExtractionStarted += Extractor_FileExtractionStarted;
            extractor.FileExtractionFinished += Extractor_FileExtractionFinished;
        }

        /// <summary>
        /// 为指定的<see cref="SevenZipExtractor"/>注销事件。
        /// </summary>
        /// <param name="extractor"></param>
        void EventLogOffFor(SevenZipExtractor extractor)
        {
            extractor.Extracting -= Extractor_Extracting;
            extractor.ExtractionFinished -= Extractor_ExtractionFinished;
            extractor.FileExtractionStarted -= Extractor_FileExtractionStarted;
            extractor.FileExtractionFinished -= Extractor_FileExtractionFinished;
        }

        private void Extractor_ExtractionFinished(object sender, EventArgs e)
        {
            //如果因取消而结束，应当删除已经导出的文件。
            if (ExtractionStatus == ExtractionStatus.Cancelled)
            {
                DeleteExtractedFiles(); //如果在直接输出模式下，则删除最终文件。
                return;
            }

            if (TempOutputPosition != ExtractionTempOutputPosition.Direct)
            {
                FileSystem.CopyDirectory(tempOutputDir, OutputDirPath, UIOption.AllDialogs, UICancelOption.DoNothing);
            }

            DeleteTempDirectory();
        }

        private void Extractor_FileExtractionStarted(object sender, FileInfoEventArgs e)
        {
            e.Cancel = cancelRequested;
            if (cancelRequested)
            {
                SetPropertyFromUIThreadAsync(nameof(ExtractionStatus), ExtractionStatus.Cancelled);
            }

            SetPropertyFromUIThreadAsync(nameof(CurrentExtractingFileName), e.FileInfo.FileName);
            SetPropertyFromUIThreadAsync(nameof(CurrentExtractingIndex), e.FileInfo.Index);
        }

        private void Extractor_FileExtractionFinished(object sender, FileInfoEventArgs e)
        {
            extractedFiles.Add(e.FileInfo);
            SetPropertyFromUIThreadAsync(nameof(ExtractedFilesCount), ExtractedFilesCount + 1).Wait(); //ExtractedFilesCount的新值依赖于旧值，应确保值更新完成再继续。

            if (ExtractedFilesCount == TotalFilesCount)
            {
                SetPropertyFromUIThreadAsync(nameof(ExtractionStatus), ExtractionStatus.Finished);
                return;
            }

            if (pauseRequested)
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
        /// 重置解压操作的所有统计数据和信息，以便执行新的解压操作。
        /// </summary>
        void ResetExtractionStatus()
        {
            extractedFiles.Clear();

            //设置缓存文件目录
            tempOutputDir = TempOutputPosition switch
            {
                ExtractionTempOutputPosition.Direct => OutputDirPath,
                ExtractionTempOutputPosition.AppLocalCache => ApplicationData.Current.LocalCacheFolder.Path,
                _ => Path.Combine(Path.GetTempPath(), "MicaApp.7zip", Path.GetRandomFileName()) //如果未指定，也使用用户缓存目录。
            };
            Debug.WriteLine($"[{nameof(ResetExtractionStatus)}] using \"{tempOutputDir}\" as temp directory.");

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
            var propertyInfo = thisType.GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var task = App.MainDispatcherQueue.EnqueueAsync(() => propertyInfo.SetValue(this, newValue));
            return task;
        }


        /// <summary>
        /// 请求取消解压，解压操作将会在下个文件开始解压前完成取消。
        /// </summary>
        [RelayCommand]
        public void CancelWork()
        {
            cancelRequested = true;
            ExtractionStatus = ExtractionStatus.Cancelling;
            ResumeWork(); //取消解压时，需要使暂停的解压操作继续，来取消线程(信号量)堵塞，以执行取消操作。
        }

        [RelayCommand]
        public void PauseWork()
        {
            if (!pauseRequested)
            {
                ExtractionStatus = ExtractionStatus.Pausing;
                pauseWorkSemaphore.Wait();
                pauseRequested = true;
            }
        }

        [RelayCommand]
        public void ResumeWork()
        {
            if (pauseRequested)
            {
                pauseWorkSemaphore.Release();
                pauseRequested = false;
                if (!cancelRequested)
                    ExtractionStatus = ExtractionStatus.Extracting;
            }
        }

        [RelayCommand]
        public void TogglePause()
        {
            if (pauseRequested)
                ResumeWork();
            else PauseWork();
        }

        /// <summary>
        /// 删除缓存文件目录（如果有的话）。
        /// </summary>
        void DeleteTempDirectory()
        {
            //确保没有使用直接输出模式，以防止删除解压后的最终文件。
            if (TempOutputPosition != ExtractionTempOutputPosition.Direct)
            {
                Directory.Delete(tempOutputDir, true);
                Debug.WriteLine($"[{nameof(DeleteTempDirectory)}] removed \"{tempOutputDir}\" and its sub-files.");
            }
        }

        /// <summary>
        /// 删除解压最终输出的文件（仅直接输出模式）。
        /// </summary>
        void DeleteExtractedFiles()
        {
            if (TempOutputPosition != ExtractionTempOutputPosition.Direct) return;

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

        void InnerExtract()
        {
            SevenZipExtractor[] extractors = new SevenZipExtractor[ExtractionInfos.Count];
            int totalFilesCount = 0;
            for (int i = 0; i < extractors.Length; i++)
            {
                extractors[i] = new SevenZipExtractor(ExtractionInfos[i].ArchivePath); //为每个ExtractionInfo初始化解压对象。
                totalFilesCount += extractors[i].ArchiveFileData.Count;  //获取总文件个数。
            }
            for (int i = 0; i < extractors.Length; i++)
            {
                var extractor = extractors[i];
                EventStartupFor(extractor);

                SetPropertyFromUIThreadAsync(nameof(TotalFilesCount), totalFilesCount);
                SetPropertyFromUIThreadAsync(nameof(ExtractionStatus), ExtractionStatus.Extracting);

                extractor.ExtractFiles(OutputDirPath, ExtractionInfos[i].FileIndexsToExtract);
                EventLogOffFor(extractor);
                extractor.Dispose();
            }
        }

        [RelayCommand]
        public async Task ExtractAsync()
        {
            await Task.Run(InnerExtract);
        }
    }

    /// <summary>
    /// 表示当前导出过程的状态。
    /// </summary>
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

    /// <summary>
    /// 表示导出时使用的临时文件中转目录。
    /// </summary>
    public enum ExtractionTempOutputPosition
    {
        Default = 0,
        /// <summary>
        /// 不使用中转目录。
        /// </summary>
        Direct,
        /// <summary>
        /// 使用用户缓存目录。
        /// </summary>
        TempFolder,
        /// <summary>
        /// 使用应用本地缓存目录。
        /// </summary>
        AppLocalCache
    }
}
