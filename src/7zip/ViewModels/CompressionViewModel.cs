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

        /// <summary>
        /// 10.11  FireFly 暂时废弃，后续再优化调整
        /// </summary>
        /// <param name="compressMethod"></param>
        /// <param name="compressMode"></param>
        /// <returns></returns>
        public async Task CompressCompressAsync(CompressionMethod compressMethod = CompressionMethod.BZip2, CompressionMode compressMode = CompressionMode.Create)
        {
            compressor.CompressionLevel = CompressionLevel.High;
            compressor.CompressionMode = compressMode;
            compressor.ZipEncryptionMethod = ZipEncryptionMethod.ZipCrypto;
            compressor.CompressionMethod = compressMethod;
            if (!string.IsNullOrEmpty(TargetArchivePath) && TargetFilePaths != null && TargetFilePaths.Count > 0)
            {
                //接口有点问题，传入word.zip， 输出word  没有后缀，等有空再调试
                await compressor.CompressFilesAsync(TargetArchivePath, TargetFilePaths.ToArray());
            }
        }

        /// <summary>
        /// 右键菜单压缩方法
        /// </summary>
        /// <param name="sourceFiles"></param>
        /// <param name="method"></param>
        public void CompressFiles(List<string> sourceFiles,string extension = "7z")
        {
            if(sourceFiles!=null&&sourceFiles.Count>0)
            {
                var testpath = sourceFiles.First();
                string outputName = Path.GetFileNameWithoutExtension(testpath) + "."+ extension;
                string outputPath = Path.Combine(Path.GetDirectoryName(testpath), outputName);
                using (FileStream ostream = new FileStream((outputPath),FileMode.Create,FileAccess.Write))
                {
                    using (FileStream istream = new FileStream((testpath), FileMode.Open, FileAccess.Read))
                    {
                        Dictionary<string,Stream> dict = new Dictionary<string, Stream>{ { testpath,istream} };
                        compressor.CompressionMethod = CompressionMethod.BZip2;
                        compressor.CompressionLevel = CompressionLevel.High;
                        //TODO: 内存不足是补充提示弹窗，补充进度对话框
                        compressor.CompressStreamDictionary(dict, ostream);
                    }
                }
            }
        }

        public void SetTargetArchivePath(string filePath)
        {
            this.TargetArchivePath = filePath;
        }

        public void SetTargetSingleFilePath(string targetPath)
        {
            this.TargetSingleFilePath = targetPath;
        }

        public void SetTargetFilePaths(List<string> filePaths)
        {
            TargetFilePaths = new ObservableCollection<string>(filePaths);
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
