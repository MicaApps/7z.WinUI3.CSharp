using _7zip.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _7zip.Models
{
    ///// <summary>
    ///// Model for Extract and Compress Option
    ///// </summary>
    //internal partial class OptionModel : ProgressViewModelBase
    //{
    //    [ObservableProperty]
    //    private string optionTitle;


    //    [ObservableProperty]
    //    private string optionProcessingText;

    //    [ObservableProperty]
    //    private string succeedText;

    //    [ObservableProperty]
    //    private string cancelText;

    //    [ObservableProperty]
    //    private string cancelingText;

    //    [ObservableProperty]
    //    private string pausedText;

    //    [ObservableProperty]
    //    private string pausingText;

    //    [ObservableProperty]
    //    private int totalFilesCount;

    //    [ObservableProperty]
    //    private int optionedFileCount;

    //    [ObservableProperty]
    //    private float optionPercentage;

    //    private string optionFileName;

    //    public string OptionFileName
    //    {
    //        get => optionFileName;
    //        set => SetProperty(ref optionFileName, value);
    //    }

    //    private OpetionStatus extractionStatus = OpetionStatus.Pausing;


    //    public OpetionStatus ExtractionStatus
    //    {
    //        get => extractionStatus;
    //        set => SetProperty(ref extractionStatus, value);
    //    }

    //    /// <summary>
    //    /// 获取或设置一个值，指示了当前解压操作是否正在暂停。
    //    /// </summary>
    //    public bool IsPausing => ExtractionStatus == OpetionStatus.Pausing;


    //    /// <summary>
    //    /// 获取或设置一个值，指示了当前解压操作是否已暂停。
    //    /// </summary>
    //    public bool IsPaused => ExtractionStatus == OpetionStatus.Paused;

    //    /// <summary>
    //    /// 获取或设置一个值，指示了正在取消（但还未完成取消）解压操作。
    //    /// </summary>
    //    public bool IsCancelling => ExtractionStatus == OpetionStatus.Cancelling;

    //    /// <summary>
    //    /// 获取或设置一个值，指示了解压操作是否被取消。
    //    /// </summary>
    //    public bool IsCancelled => ExtractionStatus == OpetionStatus.Cancelled;

    //    /// <summary>
    //    /// 获取或设置一个值，指示了解压操作是否完成。
    //    /// </summary>
    //    public bool IsFinished => ExtractionStatus == OpetionStatus.Finished;

    //    public bool IsProcessing => ExtractionStatus == OpetionStatus.OptionProcessing;


    //    private RelayCommand? pauseOption;

    //    public RelayCommand PaussOptionCommand
    //    {
    //        get => pauseOption;
    //        set => SetProperty(ref pauseOption, value);
    //    }

    //    private RelayCommand? cancelOption;

    //    public RelayCommand CancelOptionCommand
    //    {
    //        get => cancelOption;
    //        set => SetProperty(ref cancelOption, value);
    //    }

    //    public OptionModel()
    //    {
    //        this.PropertyChanged += OptionModel_PropertyChanged; 
    //    }

    //    private void OptionModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    //    {
    //        switch (e.PropertyName)
    //        {
    //            case nameof(ExtractionStatus):
    //                OnPropertyChanged(nameof(IsPausing));
    //                OnPropertyChanged(nameof(IsPaused));
    //                OnPropertyChanged(nameof(IsCancelling));
    //                OnPropertyChanged(nameof(IsCancelled));
    //                OnPropertyChanged(nameof(IsProcessing));
    //                OnPropertyChanged(nameof(IsFinished));
    //                break;
    //        }
    //    }
    //}
}
