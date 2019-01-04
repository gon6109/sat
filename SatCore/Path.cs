using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    public class Path
    {
        /// <summary>
        /// 絶対パスから相対パスに変換する
        /// </summary>
        /// <param name="path">変換するパス</param>
        /// <param name="rootPath">基準となるパス</param>
        /// <returns>相対パス</returns>
        public static string GetRelativePath(string path, string rootPath)
        {
            try
            {
                Uri relativeUri = (new Uri(rootPath)).MakeRelativeUri(new Uri(path));
                return relativeUri.ToString();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
