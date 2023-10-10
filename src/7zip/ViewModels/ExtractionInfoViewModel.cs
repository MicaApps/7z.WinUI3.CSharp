using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7zip.ViewModels
{
    internal partial class ExtractionInfoViewModel :ObservableObject
    {
        private string archivePath;
        private int[] fileIndexesToExtract;

        public string ArchivePath
        {
            get => archivePath;
            set => SetProperty(ref archivePath, value);
        }
        public int[] FileIndexsToExtract
        {
            get => fileIndexesToExtract;
            private set => SetProperty(ref fileIndexesToExtract, value);
        }
        public bool ShouldExtractAllFiles => !FileIndexsToExtract?.Any() ?? true;

        public ExtractionInfoViewModel(string archivePath = null, int[] fileIndexsToExtract = null)
        {
            this.archivePath = archivePath;
            this.fileIndexesToExtract = fileIndexsToExtract;
        }
    }
}
