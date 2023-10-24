using _7zip.Models;
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
using System.Reflection;
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
        bool isWorking = false;
        bool pauseRequested = false;
        bool cancelRequested = false;
        string tempOutputDir; //中转目录的路径。
        SemaphoreSlim pauseWorkSemaphore = new(1, 1);
        List<ArchiveFileInfo> extractedFiles = new(10); //已经导出的压缩文件内部文件信息。
        #endregion

        #region MVVM Observable Back-Fields
        ExtractionTempOutputPosition tempOutputPosition;
        string outputDirPath;
        float extractPercentage;
        ArchiveFileInfo currentExtractingArchiveInfo;
        int totalFilesCount;
        int extractedFilesCount;
        int extractedArchivesCount;
        ExtractionInfoViewModel currentExtractionInfo;
        public OptionModel model = new OptionModel();
        #endregion

        #region MVVM Properties
        /// <summary>
        /// 获取要导出的压缩文件的路径。
        /// </summary>
        public List<ExtractionInfoViewModel> ExtractionInfos { get; private set; }

        /// <summary>
        /// 获取或设置解压使用的中转缓存目录位置。
        /// </summary>
        public ExtractionTempOutputPosition TempOutputPosition
        {
            get => tempOutputPosition;
            set { if (!isWorking) SetProperty(ref tempOutputPosition, value); }
        }

        /// <summary>
        /// 获取或设置解压的目标输出路径。
        /// </summary>
        public string OutputDirPath
        {
            get => outputDirPath;
            set { if (!isWorking) SetProperty(ref outputDirPath, value); }
        }

        /// <summary>
        /// 获取或设置当前解压操作的总进度(0f-1f)。
        /// </summary>
        public float ExtractPercentage
        {
            get => extractPercentage;
            private set => SetProperty(ref extractPercentage, value);
        }

        /// <summary>
        /// 获取已解压完毕的压缩包个数。
        /// </summary>
        public int ExtractedArchivesCount
        {
            get => extractedArchivesCount;
            private set => SetProperty(ref extractedArchivesCount, value);
        }

        /// <summary>
        /// 获取当前的导出信息。
        /// </summary>
        public ExtractionInfoViewModel CurrentExtractionInfo
        {
            get => currentExtractionInfo;
            private set => SetProperty(ref currentExtractionInfo, value);
        }

        //public ArchiveFileInfo CurrentExtractingArchiveInfo
        //{
        //    get => currentExtractingArchiveInfo;
        //    private set => SetProperty(ref currentExtractingArchiveInfo, value);
        //}

        public int TotalArchivesCount => ExtractionInfos.Count;
        #endregion
        public ExtractionViewModel(IEnumerable<ExtractionInfoViewModel> extractionInfos)
        {
            ExtractionInfos = extractionInfos.ToList() ?? throw new ArgumentNullException(nameof(extractionInfos));
            //this.PropertyChanged += ExtractViewModel_PropertyChanged;
            InitModel();
        }

        private void InitModel()
        {
            model.OptionTitle = "解压";
            model.OptionProcessingText = "正在解压";
            model.SucceedText = "解压成功";
            model.PausingText = "正在暂停......";
            model.PausedText = "解压已暂停";
            model.CancelText = "解压已取消";
            model.CancelingText = "正在取消......";
            model.PaussOptionCommand = new RelayCommand(TogglePause);
            model.CancelOptionCommand = new RelayCommand(CancelWork);
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


        /// <summary>
        /// 单个压缩文件解压完成时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Extractor_ExtractionFinished(object sender, EventArgs e)
        {
            //if (model.ExtractionStatus != OpetionStatus.Cancelled)
            //    SetPropertyFromUIThreadAsync(nameof(model.OptionedFileCount), model.OptionedFileCount + 1).Wait();
            //如果因取消而结束，应当删除已经导出的文件。
            if (model.ExtractionStatus == OpetionStatus.Cancelled)
            {
                DeleteExtractedFiles(); //如果在直接输出模式下，则删除最终文件。
                return;
            }
        }

        private void Extractor_FileExtractionStarted(object sender, FileInfoEventArgs e)
        {
            e.Cancel = cancelRequested;
            if (cancelRequested)
            {
                SetPropertyFromUIThreadAsync(nameof(model.ExtractionStatus), OpetionStatus.Cancelled);
            }

            //SetPropertyFromUIThreadAsync(nameof(CurrentExtractingArchiveInfo), e.FileInfo);
            SetPropertyFromUIThreadAsync(nameof(model.OptionFileName),e.FileInfo.FileName);
        }

        private void Extractor_FileExtractionFinished(object sender, FileInfoEventArgs e)
        {
            extractedFiles.Add(e.FileInfo);
            SetPropertyFromUIThreadAsync(nameof(model.OptionedFileCount), model.OptionedFileCount + 1).Wait(); //ExtractedFilesCount的新值依赖于旧值，应确保值更新完成再继续。
           // ExtractedArchivesCount++;
            if (model.OptionedFileCount == model.TotalFilesCount)
            {
                OnExtractionSucceeded();
                SetPropertyFromUIThreadAsync(nameof(model.ExtractionStatus), OpetionStatus.Finished);
                return;
            }

            if (pauseRequested)
            {
                SetPropertyFromUIThreadAsync(nameof(model.ExtractionStatus), OpetionStatus.Paused);
                pauseWorkSemaphore.Wait();
                pauseWorkSemaphore.Release();
            }
        }

        private void Extractor_Extracting(object sender, ProgressEventArgs e)
        {
            float percentage = (model.OptionedFileCount + e.PercentDone / 100f) / model.TotalFilesCount;
            percentage = MathF.Round(percentage, 2);
            SetPropertyFromUIThreadAsync(nameof(model.OptionPercentage), percentage);
        }

        //private void ExtractViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    switch(e.PropertyName)
        //    {
        //        case nameof(model.ExtractionStatus):
        //            OnPropertyChanged(nameof(model.IsPausing));
        //            OnPropertyChanged(nameof(model.IsPaused));
        //            OnPropertyChanged(nameof(model.IsCancelling));
        //            OnPropertyChanged(nameof(model.IsCancelled));
        //            OnPropertyChanged(nameof(model.IsProcessing));
        //            OnPropertyChanged(nameof(model.IsFinished));
        //            break;
        //    }
        //}

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

            ExtractedArchivesCount = 0;
            ExtractPercentage = 0;
            model.OptionedFileCount = 0;
            model.TotalFilesCount = -1;
        }

        /// <summary>
        /// 通过反射从UI线程更新该ViewModel的属性。
        /// </summary>
        /// <param name="propertyName">属性名称，强烈建议使用nameof</param>
        /// <param name="newValue">要赋予的值</param>
        /// <param name="generateWaitTask">是否要生成该操作的Task，以便进行等待。</param>
        Task SetPropertyFromUIThreadAsync(string propertyName, object newValue, bool Updatemodel = true)
        {
            PropertyInfo propertyInfo;
            Task task;
            if (Updatemodel)
            {
                propertyInfo = model.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                task = App.MainDispatcherQueue.EnqueueAsync(() => propertyInfo?.SetValue(model, newValue));
            }
            else
            {
                propertyInfo = this.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                task = App.MainDispatcherQueue.EnqueueAsync(() => propertyInfo?.SetValue(this, newValue));
            }
            return task;
        }


        /// <summary>
        /// 请求取消解压，解压操作将会在下个文件开始解压前完成取消。
        /// </summary>
        [RelayCommand]
        public void CancelWork()
        {
            cancelRequested = true;
            model.ExtractionStatus = OpetionStatus.Cancelling;
            ResumeWork(); //取消解压时，需要使暂停的解压操作继续，来取消线程(信号量)堵塞，以执行取消操作。
        }

        [RelayCommand]
        public void PauseWork()
        {
            if (!pauseRequested)
            {
                model.ExtractionStatus = OpetionStatus.Pausing;
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
                    model.ExtractionStatus = OpetionStatus.OptionProcessing;
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
                    Debug.WriteLine($"[{nameof(Extractor_ExtractionFinished)}]:{ex.Message}");
                    //待定异常处理
                }
            }
        }

        void InnerExtract()
        {
            ResetExtractionStatus();
            SevenZipExtractor[] extractors = new SevenZipExtractor[ExtractionInfos.Count];
            int totalFilesCount = 0;
            for (int i = 0; i < extractors.Length; i++)
            {
                extractors[i] = new SevenZipExtractor(ExtractionInfos[i].ArchivePath); //为每个ExtractionInfo初始化解压对象。
                totalFilesCount += extractors[i].ArchiveFileData.Count;  //获取总文件个数。
            }
            SetPropertyFromUIThreadAsync(nameof(model.TotalFilesCount), totalFilesCount);

            for (int i = 0; i < extractors.Length; i++)
            {
                var extractor = extractors[i];
                SetPropertyFromUIThreadAsync(nameof(CurrentExtractionInfo), ExtractionInfos[i], false).Wait();
                SetPropertyFromUIThreadAsync(nameof(model.ExtractionStatus), OpetionStatus.OptionProcessing);

                EventStartupFor(extractor);
                try
                {
                    //如果extractionInfo的FileIndexesToExtract为空，则导出所有文件，否则仅导出这些文件
                    extractor.ExtractFiles(tempOutputDir,
                        CurrentExtractionInfo.ShouldExtractAllFiles ?
                        GetIndexesForExtraction(extractor) : CurrentExtractionInfo.FileIndexsToExtract);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[{nameof(InnerExtract)}]:{ex.Message}");
                }
                finally
                {
                    EventLogOffFor(extractor);
                    extractor.Dispose();
                    extractor = null;
                }

                if (model.ExtractionStatus == OpetionStatus.Cancelled)
                {
                    foreach (var e in extractors[(i + 1)..]) { e.Dispose(); } //在取消时dispose掉剩余的extractor。
                    break;
                }
            }
            OnExtractionFinishedOrCancelled();
        }

        /// <summary>
        /// 处理所有压缩文件 *成功* 解压完毕后的操作。
        /// </summary>
        void OnExtractionSucceeded()
        {
            if (TempOutputPosition != ExtractionTempOutputPosition.Direct)
            {
                FileSystem.CopyDirectory(tempOutputDir, OutputDirPath, UIOption.AllDialogs, UICancelOption.DoNothing);
            }
        }

        /// <summary>
        /// 执行解压结束时的操作。
        /// </summary>
        void OnExtractionFinishedOrCancelled()
        {
            DeleteTempDirectory();
        }

        int[] GetIndexesForExtraction(SevenZipExtractor extractor)
        {
            if (extractor is null) throw new ArgumentNullException();
            return extractor.ArchiveFileData.Select(d => d.Index).ToArray();
        }

        /// <summary>
        /// 检查参数是否有效。
        /// </summary>
        /// <returns></returns>
        bool CheckParameters()
        {
            return ExtractionInfos.Any()
                && InitializeOutputDirectory();
        }

        /// <summary>
        /// 初始化输出目录。
        /// </summary>
        /// <returns>成功返回true，否则返回false。</returns>
        bool InitializeOutputDirectory()
        {
            bool success = true;
            try
            {
                Directory.CreateDirectory(OutputDirPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{nameof(InitializeOutputDirectory)}]:{ex.Message}");
                success = false;
            }
            return success;
        }


        [RelayCommand]
        public async Task ExtractAsync()
        {
            if (!CheckParameters()) return;
            isWorking = true;
            await Task.Run(InnerExtract);
            isWorking = false;
        }
    }

    /// <summary>
    /// 表示当前导出过程的状态。
    /// </summary>
    public enum OpetionStatus
    {
        None = 0,
        OptionProcessing,
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
        /// <summary>
        /// 等同于TempFolder。
        /// </summary>
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
