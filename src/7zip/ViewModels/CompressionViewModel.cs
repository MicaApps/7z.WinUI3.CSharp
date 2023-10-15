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
using System.IO;
using Windows.ApplicationModel.VoiceCommands;
using _7zip.Views.Windows;
using Microsoft.UI.Xaml;

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

        [ObservableProperty]
        private string targetSingleFilePath;

        public CompressionViewModel()
        {
            EventStartup();
        }

        /// <summary>
        /// ContextMune Compress Function
        /// </summary>
        /// <param name="sourceFiles"></param>
        /// <param name="method"></param>
        public void CompressFiles(List<string> sourceFiles,string extension = "7z")
        {
            if(sourceFiles!=null&&sourceFiles.Count>0)
            {
                //Compress Single File
                var firstfile = sourceFiles.First();
                string outputName = Path.GetFileNameWithoutExtension(firstfile) + "."+ extension;
                if (sourceFiles.Count > 1)
                {
                    //Mutiplefile outputName
                    DirectoryInfo info = new DirectoryInfo(firstfile);
                    outputName = info.Parent.Name + "." + extension;

                }

         

                string outputPath = Path.Combine(Path.GetDirectoryName(firstfile), outputName);
                compressor.ArchiveFormat = OutArchiveFormat.SevenZip;
    
                //Tip:CompressFileAsync not working,alway create empty file
                compressor.CompressFiles(outputPath, sourceFiles.ToArray());

                //Compress Directory
                compressor.CompressionMode = CompressionMode.Append;
                compressor.PreserveDirectoryRoot = true;
                foreach (var directorypath in sourceFiles)
                {
                    if (Directory.Exists(directorypath))
                    {
                        var directoryName = Path.GetFileNameWithoutExtension(directorypath);
                        compressor.CompressDirectory(directorypath, outputPath);
                    }
                }
            }
        }


        void EventStartup()
        {
            compressor.FileCompressionStarted += Compressor_FileCompressionStarted;
            compressor.Compressing += Compressor_Compressing;
            compressor.FileCompressionFinished += Compressor_FileCompressionFinished;
            compressor.CompressionFinished += Compressor_CompressionFinished;
            this.PropertyChanged += CompressionViewModel_PropertyChanged; ;
        }

        private void Compressor_FileCompressionStarted(object sender, FileNameEventArgs e)
        {
            
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
                case nameof(CompressionMethod): compressor.CompressionMethod = this.CompressionMethod; break;
            }
        }

        private void Compressor_FileCompressionFinished(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Compressor_CompressionFinished(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Compressor_Compressing(object sender, ProgressEventArgs e)
        {
            CompressionPercentage = e.PercentDone / 100f;
        }
    }
}
