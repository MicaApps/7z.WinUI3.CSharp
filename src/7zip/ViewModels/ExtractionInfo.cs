using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7zip.ViewModels
{
    internal class ExtractionInfo
    {
        public string ArchivePath { get; private set; }
        public int[] FileIndexsToExtract { get;private set; }
        public bool ShouldExtractAllFiles => !FileIndexsToExtract.Any();

        public ExtractionInfo(string archivePath, int[] fileIndexsToExtract)
        {
            ArchivePath = archivePath;
            FileIndexsToExtract = fileIndexsToExtract ?? Array.Empty<int>();
        }
    }
}
