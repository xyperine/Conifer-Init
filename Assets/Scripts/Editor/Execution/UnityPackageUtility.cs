using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using UnityEditor;
using UnityEngine.Assertions;

namespace ProjectSetup.Editor.Execution
{
    internal static class UnityPackageUtility
    {
        public const string UNITY_PACKAGE_FILE_EXTENSION = ".unitypackage";


        public static List<string> GetPathNames(string packagePath)
        {
            Assert.IsTrue(packagePath.EndsWith(UNITY_PACKAGE_FILE_EXTENSION));
            
            List<string> paths = new List<string>();

            using FileStream fileStream = File.OpenRead(packagePath);
            using GZipInputStream gzipStream = new GZipInputStream(fileStream);
            using TarInputStream tarStream = new TarInputStream(gzipStream, Encoding.UTF8);

            while (tarStream.GetNextEntry() is { } entry)
            {
                if (!entry.Name.EndsWith("/pathname"))
                {
                    continue;
                }

                using StreamReader reader = new StreamReader(tarStream, Encoding.UTF8, false, 1024, true);

                string pathname = reader.ReadLine();
                
                paths.Add(pathname?.Trim());
            }
            
            return paths;
        }


        public static bool AllPluginAssetsAlreadyImported(string packagePath)
        {
            List<string> paths = GetPathNames(packagePath);
            return paths.All(AssetDatabase.AssetPathExists);
        }
    }
}