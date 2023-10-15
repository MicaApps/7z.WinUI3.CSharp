using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace _7zip.Helpers
{
    /// <summary>
    /// File Option Helper
    /// </summary>
    public static class CacheFileHelper
    {
        /// <summary>
        /// Get Context Menu Start Command Files
        /// </summary>
        /// <param name="starttype">start command type,like -extract,-7z-zip,-email</param>
        public static List<string> GetStartCommandFilePaths(string starttype)
        {
            List<string> sourceFiles = new List<string>();
            string cachFileName = "";
            switch(starttype)
            {
                case "-extract":
                    cachFileName = "ExtractPathsTempFile.out";
                    break;
                case "-7z":
                    cachFileName = "Compress7zPathsTempFile.out";
                    break;
                case "-zip":
                    cachFileName = "CompressZipPathsTempFile.out";
                    break;
                case "-email":
                    cachFileName = "CompressEmailPathsTempFile.out";
                    break;
            }
            string cachFilePath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path,cachFileName);
            if (File.Exists(cachFilePath))
            {
                sourceFiles = File.ReadAllLines(cachFilePath).ToList();
                //Delete First Line String,It's File Path Count
                sourceFiles.RemoveAt(0);
            }
            return sourceFiles;
        }
    }
}
