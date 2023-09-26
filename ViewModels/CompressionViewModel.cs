using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SevenZip.Compression;
using SevenZip;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace _7zip.ViewModels
{
    internal partial class CompressionViewModel : ObservableObject
    {
        SevenZipCompressor compressor = new();

        [ObservableProperty]
        CompressionMode compressionMode = CompressionMode.Create;

        [ObservableProperty]
        CompressionMethod compressionMethod = CompressionMethod.Lzma;

        [ObservableProperty]
        float compressionPercentage;

        [ObservableProperty]
        string targetArchivePath;

        [ObservableProperty]
        ObservableCollection<string> targetFilePaths = new();

        [RelayCommand]
        public async Task CompressAsync()
        {
            await compressor.CompressFilesAsync(TargetArchivePath, TargetFilePaths.ToArray());
        }

        public CompressionViewModel()
        {
            EventStartup();
        }

        void EventStartup()
        {
            compressor.Compressing += Compressor_Compressing;
            compressor.FileCompressionFinished += Compressor_FileCompressionFinished;
            compressor.CompressionFinished += Compressor_CompressionFinished;
            this.PropertyChanged += CompressionViewModel_PropertyChanged; ;
        }

        /// <summary>
        /// TODO:将ViewModel属性更改应用到SevenZip的compressor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompressionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CompressionMode): compressor.CompressionMode = this.CompressionMode; break;
                case nameof(CompressionMethod):compressor.CompressionMethod = this.CompressionMethod; break;
            }
        }

        private void Compressor_FileCompressionFinished(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Compressor_CompressionFinished(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Compressor_Compressing(object sender, ProgressEventArgs e)
        {
            CompressionPercentage = e.PercentDone / 100f;
        }
    }
}
