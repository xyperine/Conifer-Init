using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConiferInit.Editor.Configuration;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ConiferInit.Editor
{
    internal static class MenuOptions
    {
#if CONIFER_INIT_DEV
        private const string WINDOW_MENU_PATH = "Tools/Conifer Init/Window";
        private const string EXPORT_MENU_PATH = "Tools/Conifer Init/Export";
        private const string EXPORT_WITHOUT_TESTS_MENU_PATH = "Tools/Conifer Init/Export (no tests)";
        private const string OPEN_PROFILE_STORAGE_MENU_PATH = "Tools/Conifer Init/Open profiles storage";
#else
        private const string WINDOW_MENU_PATH = "Tools/Conifer Init";
#endif
        
        
        [MenuItem(WINDOW_MENU_PATH, false, 0)]
        private static void ShowWindow()
        {
            if (!EditorWindow.HasOpenInstances<Window>())
            {
                CompositionRoot.CreateWindow();
            }
            else
            {
                EditorWindow.FocusWindowIfItsOpen<Window>();
            }
        }


#if CONIFER_INIT_DEV
        [Flags]
        private enum ExportOptions
        {
            Default = 0,
            WithoutTests = 1,
        }
        
        
        [MenuItem(EXPORT_MENU_PATH, false, 100)]
        private static void ExportPackageDefault()
        {
            ExportPackage(ExportOptions.Default);
        }


        [MenuItem(EXPORT_WITHOUT_TESTS_MENU_PATH, false, 101)]
        private static void ExportPackageWithoutTests()
        {
            ExportPackage(ExportOptions.WithoutTests);
        }


        private static async void ExportPackage(ExportOptions options)
        {
            string[] auxFileNames =
                {"LICENSE", "CHANGELOG.md", "NOTICES", "User Guide.pdf"};
            
            // Move all auxiliary files in
            foreach (string fileName in auxFileNames)
            {
                string path = Path.Combine(Environment.CurrentDirectory, fileName);
                File.Copy(path, Path.Combine(Application.dataPath, fileName));

                AssetDatabase.Refresh();
                
                string result = AssetDatabase.MoveAsset(Path.Combine("Assets", fileName),
                    Path.Combine("Assets", "Plugins", "ConiferInit", fileName));

                if (!string.IsNullOrWhiteSpace(result))
                {
                    Debug.Log(result);
                }
                
                AssetDatabase.Refresh();
            }
            
            AssetDatabase.Refresh();
            
            // Export
            string pluginPath = Path.Combine(Application.dataPath, "Plugins", "ConiferInit");
            IEnumerable<string> potentialFiles = Directory.GetFileSystemEntries(pluginPath, "*", SearchOption.AllDirectories);
            potentialFiles = potentialFiles.Where(e => !e.EndsWith(".meta"));
            if (options.HasFlag(ExportOptions.WithoutTests))
            {
                string testsPath = Path.Combine(pluginPath, "Scripts", "Tests");
                potentialFiles = potentialFiles.Where(e => !e.Contains(testsPath));
            }
            potentialFiles = potentialFiles.Select(e => e.Replace(Application.dataPath, "Assets"));
            string[] files = potentialFiles.ToArray();

            List<string> pathsToExport = new List<string>();
            pathsToExport.Add(Path.Combine("Assets", "Editor", "ICSharpCode.SharpZipLib.dll"));
            pathsToExport.AddRange(files);
            
            AssetDatabase.ExportPackage(pathsToExport.ToArray(),
                $"Conifer_Init_{PlayerSettings.bundleVersion}.unitypackage",
                ExportPackageOptions.Default);

            await Task.Delay(1000);
            
            // Delete auxiliary files
            foreach (string fileName in auxFileNames)
            {
                string path = Path.Combine("Assets", "Plugins", "ConiferInit", fileName);
                AssetDatabase.DeleteAsset(path);
            }
        }


        [MenuItem(OPEN_PROFILE_STORAGE_MENU_PATH, false, 102)]
        private static void OpenProfilesStorage()
        {
            Process.Start(SettingsProfilePersistency.StoragePath);
        }
#endif
    }
}