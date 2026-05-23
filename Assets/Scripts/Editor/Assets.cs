using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace ProjectSetup.Editor
{
    // TODO: Add support for interactive import
    // TODO: Automatically move imported assets to the plugins folder
    internal static class Assets
    {
        private const string UNITY_PACKAGE_FILE_EXTENSION = ".unitypackage";
            
            
        public static void Import(string assetName, string folder, bool interactive)
        {
            string basePath;
            if (Environment.OSVersion.Platform is PlatformID.MacOSX or PlatformID.Unix)
            {
                string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                basePath = Path.Combine(homeDirectory, "Library/Unity/Asset Store-5.x");
            }
            else
            {
                string defaultPath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unity");
                basePath = Path.Combine(EditorPrefs.GetString("AssetStoreCacheRootPath", defaultPath),
                    "Asset Store-5.x");
            }

            assetName = assetName.EndsWith(UNITY_PACKAGE_FILE_EXTENSION)
                ? assetName
                : assetName + UNITY_PACKAGE_FILE_EXTENSION;

            string fullPath = Path.Combine(basePath, folder, assetName);

            Import(fullPath, interactive);
        }


        public static void Import(string fullPath, bool interactive)
        {
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"The asset package was not found at the path: {fullPath}");
            }
                
            AssetDatabase.ImportPackage(fullPath, interactive);
        }
            
            
        public static void Import(IEnumerable<AssetInfo> assets)
        {
            AssetsImporter.Begin(assets);
        }
    }
}