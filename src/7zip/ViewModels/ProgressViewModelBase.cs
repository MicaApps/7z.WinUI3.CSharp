using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7zip.ViewModels
{
    /// <summary>
    /// 为基于进度条的工作提供抽象，并提供状态文本托管功能。
    /// </summary>
    internal class ProgressViewModelBase : MultiThreadBindableBase
    {
        bool autoManagePrimaryText;

        private string title;
        private string primaryText;
        private string secondaryText;
        private string progressPrefixText;
        private long currentProgressValue;
        private long maxProgressValue;
        private float progressPercentage;
        private ProgressStatus status;

        public string Title { get => title; protected set => SetProperty(ref title, value); }
        public string PrimaryText { get => primaryText; protected set => SetProperty(ref primaryText, value); }
        public string SecondaryText { get => secondaryText; protected set => SetProperty(ref secondaryText, value); }
        public string ProgressPrefixText { get => progressPrefixText; protected set => SetProperty(ref progressPrefixText, value); }
        public long CurrentProgressValue { get => currentProgressValue; protected set => SetProperty(ref currentProgressValue, value); }
        public long MaxProgressValue { get => maxProgressValue; protected set => SetProperty(ref maxProgressValue, value); }
        public float ProgressPercentage { get => progressPercentage; protected set => SetProperty(ref progressPercentage, value); }
        public ProgressStatus ProgressStatus { get => status; protected set => SetProperty(ref status, value); }
        /// <summary>
        /// 托管<see cref="PrimaryText"/>时所用的信息。
        /// 通过设置该字段的成员属性，可以决定在工作状态变更时所使用的更新文本。
        /// </summary>
        public readonly ProgressStatusTextManagementInfo TextManagementInfo = new();
        public RelayCommand TogglePauseCommand { get; protected set; }
        public RelayCommand CancelCommand { get; protected set; }

        //<summary>
        /// 获取或设置一个值，指示了当前工作状态是否为“正在暂停”。
        /// </summary>
        public bool IsPausing => ProgressStatus == ProgressStatus.Pausing;


        /// <summary>
        /// 获取或设置一个值，指示了当前工作状态是否为“已暂停”。
        /// </summary>
        public bool IsPaused => ProgressStatus == ProgressStatus.Paused;

        /// <summary>
        /// 获取或设置一个值，指示了当前工作状态是否为“正在取消”。
        /// </summary>
        public bool IsCancelling => ProgressStatus == ProgressStatus.Cancelling;

        /// <summary>
        /// 获取或设置一个值，指示了当前工作状态是否为“已取消”。
        /// </summary>
        public bool IsCancelled => ProgressStatus == ProgressStatus.Cancelled;

        /// <summary>
        /// 获取或设置一个值，指示了当前工作状态是否为“已完成”。
        /// </summary>
        public bool IsFinished => ProgressStatus == ProgressStatus.Finished;

        public bool IsProcessing => ProgressStatus == ProgressStatus.Processing;
        
        /// <summary>
        /// 创建新的<see cref="ProgressViewModelBase"/>实例。
        /// </summary>
        /// <param name="managePrimaryText">是否需要托管<see cref="PrimaryText"/>状态文本显示。 如果托管文本，则还需要设置<see cref="TextManagementInfo"/>的属性值，当<see cref="ProgressStatus"/>变更时，将会自动更新该文本。</param>
        public ProgressViewModelBase(bool managePrimaryText = false)
        {
            this.autoManagePrimaryText = managePrimaryText;
            this.PropertyChanged += ProgressViewModelBase_PropertyChanged;
            if (managePrimaryText)
            {
                TextManagementInfo = new ProgressStatusTextManagementInfo();
                TextManagementInfo.PropertyChanged += TextManagementInfo_PropertyChanged;
            }
        }

        private void ProgressViewModelBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ProgressStatus):
                    UpdatePrimaryText();
                    OnPropertyChanged(nameof(IsPausing));
                    OnPropertyChanged(nameof(IsPaused));
                    OnPropertyChanged(nameof(IsCancelling));
                    OnPropertyChanged(nameof(IsCancelled));
                    OnPropertyChanged(nameof(IsProcessing));
                    OnPropertyChanged(nameof(IsFinished));
                    break;
            }
        }

        /// <summary>
        /// 当<see cref="TextManagementInfo"/>的属性被更改时被调用。在此更新PrimaryText。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void TextManagementInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdatePrimaryText();
        }
        private void UpdatePrimaryText()
        {
            if (!autoManagePrimaryText) return;
            PrimaryText = ProgressStatus switch
            {
                ProgressStatus.Processing => TextManagementInfo.ProcessingText,
                ProgressStatus.Finished => TextManagementInfo.FinishedText,
                ProgressStatus.Pausing => TextManagementInfo.PausingText,
                ProgressStatus.Paused => TextManagementInfo.PausedText,
                ProgressStatus.Cancelling => TextManagementInfo.CancellingText,
                ProgressStatus.Cancelled => TextManagementInfo.CancelledText,
                _ => string.Empty
            };
        }
    }

    internal partial class ProgressStatusTextManagementInfo : ObservableObject
    {
        [ObservableProperty]
        string processingText;
        [ObservableProperty]
        string finishedText;
        [ObservableProperty]
        string pausingText;
        [ObservableProperty]
        string pausedText;
        [ObservableProperty]
        string cancellingText;
        [ObservableProperty]
        string cancelledText;

        internal ProgressStatusTextManagementInfo()
        {

        }
    }

    /// <summary>
    /// 表示当前进度过程的状态。
    /// </summary>
    public enum ProgressStatus
    {
        None = 0,
        Processing,
        Finished,
        Pausing,
        Paused,
        Cancelling,
        Cancelled
    }
}
