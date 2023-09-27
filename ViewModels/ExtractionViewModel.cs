﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7zip.ViewModels
{
    internal partial class ExtractionViewModel : ObservableObject
    {
        #region Private Fields
        SevenZipExtractor extractor;
        int[] filesIndexToExtract;
        bool isExtractingWholeArchive; //是否正在直接导出整个压缩包
        bool shouldCancelWork = false;
        #endregion

        #region MVVM Properties
        /// <summary>
        /// 获取压缩文件的路径。
        /// </summary>
        public string ArchivePath => extractor.FileName;

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
        /// 获取或设置一个值，指示了解压操作是否完成。
        /// </summary>
        [ObservableProperty]
        bool isExtractionFinished = false;

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
        #endregion
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
            extractor.ExtractionFinished += Extractor_ExtractionFinished;
            extractor.FileExtractionStarted += Extractor_FileExtractionStarted;
            extractor.FileExtractionFinished += Extractor_FileExtractionFinished;
            this.PropertyChanged += ExtractViewModel_PropertyChanged;
        }

        private void Extractor_ExtractionFinished(object sender, EventArgs e)
        {

        }

        private void Extractor_FileExtractionStarted(object sender, FileInfoEventArgs e)
        {
            e.Cancel = shouldCancelWork;

            CurrentExtractingFileName = e.FileInfo.FileName;
            CurrentExtractingIndex = e.FileInfo.Index;
        }

        private void Extractor_FileExtractionFinished(object sender, FileInfoEventArgs e)
        {
            ExtractedFilesCount++;

            e.Cancel = shouldCancelWork;

            if (ExtractedFilesCount == TotalFilesCount)
                IsExtractionFinished = true;
        }

        private void Extractor_Extracting(object sender, ProgressEventArgs e)
        {
            ExtractPercentage = e.PercentDone / 100f;
        }

        private void ExtractViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// 重置解压操作的所有统计数据，以便执行新的解压操作。
        /// </summary>
        void ResetExtractionStatistics()
        {
            ExtractPercentage = 0;
            ExtractedFilesCount = 0;
            TotalFilesCount = -1;
        }


        [RelayCommand]
        public void CancelWork()
        {
            shouldCancelWork = true;
        }

        [RelayCommand]
        public async Task ExtractAsync()
        {
            ResetExtractionStatistics();
            if (FileIndexes.Any())
            {
                filesIndexToExtract = FileIndexes.ToArray();
            }
            else
            {
                filesIndexToExtract = extractor.ArchiveFileData.Select(d => d.Index).ToArray();
            }
            TotalFilesCount = filesIndexToExtract.Length;

            await extractor.ExtractFilesAsync(OutputDirPath, filesIndexToExtract);
        }
    }
}
