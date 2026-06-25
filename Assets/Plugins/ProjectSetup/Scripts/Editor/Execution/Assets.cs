using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectSetupTool.Editor.Configuration;
using UnityEditor;
using UnityEngine.Assertions;

namespace ProjectSetupTool.Editor.Execution
{
    internal static class Assets
    {
        public static void Import(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"The asset package was not found at the path: {fullPath}");
            }
                
            AssetDatabase.ImportPackage(fullPath, false);
        }
            
            
        public static void ImportInteractive(IEnumerable<AssetImportEntry> assets)
        {
            Assert.IsTrue(assets.Any());
            
            SequentialAssetsImporter.Begin(assets);
        }
    }
}