using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConiferInit.Editor.Configuration;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ConiferInit.Editor.Execution
{
    internal static class Assets
    {
        public static void ImportNonInteractive(IEnumerable<AssetImportEntry> assets)
        {
            Assert.IsTrue(assets.Any());
            
            foreach (AssetImportEntry asset in assets)
            {
                Import(asset.Path);
            }
        }


        public static void Import(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"The asset package was not found at the path: {fullPath}");
            }
            
            if (UnityPackageUtility.AllPluginAssetsAlreadyImported(fullPath))
            {
                return;
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