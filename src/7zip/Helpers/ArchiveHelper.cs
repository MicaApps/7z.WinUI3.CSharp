using SevenZip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7zip.Helpers
{
    /// <summary>
    /// 存储了压缩文件内部文件的结构信息。
    /// </summary>
    internal class ArchiveStructure
    {
        public static readonly string RootName = "root";
        public static readonly string RootExpression = "\\root";

        /// <summary>
        /// 获取当前项目的路径表达式，格式如下： <code>\root\[Paths]</code>
        /// </summary>
        public string Expression { get; init; }
        /// <summary>
        /// 获取当前文件或文件夹的名字。
        /// </summary>
        public string Name => Expression.Replace('/','\\').Split('\\').Last();
        /// <summary>
        /// 获取一个值，指示了当前项目是否为文件夹。
        /// </summary>
        public bool IsDirectory { get; init; }
        /// <summary>
        /// 获取一个值，指示了当前项目是否为根项目（根项目的）。
        /// </summary>
        public bool IsRoot { get; init; }
        /// <summary>
        /// 当前项目的子项目。如果当前项目不是文件夹，则该值为null。
        /// </summary>
        public List<ArchiveStructure> SubStructures { get; init; }

        public override bool Equals(object obj)
        {
            if(obj is ArchiveStructure structure)
            {
                return structure.IsDirectory == this.IsDirectory
                    && structure.Expression == this.Expression;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Expression.GetHashCode() | IsDirectory.GetHashCode();
        }
    }

    /// <summary>
    /// 提供压缩文件的帮助方法。
    /// </summary>
    internal static class ArchiveHelper
    {

        static void ThrowForNullArgument(params object[] obj)
        {
            if(obj.Contains(null)) throw new ArgumentNullException("One or more argument objects are null.");
        }

        /// <summary>
        /// 获取一个压缩文件内所有文件索引构成的数组。
        /// </summary>
        /// <param name="extractor"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static int[] GetIndexesForAllFiles(SevenZipExtractor extractor)
        {
            ThrowForNullArgument(extractor);
            return extractor.ArchiveFileData.Select(d => d.Index).ToArray();
        }

        /// <summary>
        /// 获取压缩文件的文件结构。
        /// </summary>
        /// <param name="extractor">引用了压缩文件的解压对象。</param>
        /// <returns>返回根文件结构的对象，该对象包含最外层的文件或文件夹项目。</returns>
        public static ArchiveStructure GetArchiveFileStructure(SevenZipExtractor extractor)
        {
            ThrowForNullArgument(extractor);
            string rootExp = ArchiveStructure.RootExpression;
            ArchiveStructure rootStructure = new ArchiveStructure() 
            {IsRoot = true, IsDirectory = true, Expression = rootExp, SubStructures = new List<ArchiveStructure>() };

           
            foreach (var fileData in extractor.ArchiveFileData)
            {
                ArchiveStructure structure = rootStructure;
                string[] sections = fileData.FileName.Replace('/', '\\').Split('\\'); //目录分段
                for (int i = 0; i < sections.Length; i++)
                {
                    string sectionName = sections[i];
                    bool isDir = fileData.IsDirectory || i < sections.Length - 1;
                    ArchiveStructure subStructure = structure.SubStructures.FirstOrDefault(s => s.Name == sectionName && s.IsDirectory == isDir);
                    if (subStructure is null)
                    {
                        StringBuilder expressionBuilder = new StringBuilder();
                        expressionBuilder.Append(rootExp);
                        foreach (var sec in sections[..(i + 1)])
                        {
                            expressionBuilder.Append('\\');
                            expressionBuilder.Append(sec);
                        }
                        subStructure = new ArchiveStructure()
                        { Expression = expressionBuilder.ToString(),
                            IsDirectory = isDir, 
                            IsRoot = false,
                            SubStructures = isDir? new List<ArchiveStructure>() : null };

                        structure.SubStructures.Add(subStructure);
                    }
                    structure = subStructure;
                }
            }
            return rootStructure;
        }

        /// <summary>
        /// 判断压缩文件的最外层项目是否为单个文件夹。
        /// </summary>
        /// <param name="extractor"></param>
        /// <returns></returns>
        public static bool IsInSingleDirStructure(SevenZipExtractor extractor)
        {
            ThrowForNullArgument(extractor);
            return GetArchiveFileStructure(extractor).SubStructures.Count == 1;
        }
    }
}
