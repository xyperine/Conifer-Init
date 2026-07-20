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
        [MenuItem("Tools/Conifer Init")]
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
        [MenuItem("Tools/Open Conifer Init profiles storage")]
        private static void OpenProfilesStorage()
        {
            Process.Start(SettingsProfilePersistency.StoragePath);
        }


        [MenuItem("Tools/Export Conifer Init")]
        private static async void ExportPackage()
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
                
                Debug.Log(result);
                
                AssetDatabase.Refresh();
            }
            
            AssetDatabase.Refresh();
            
            // Export
            string pluginPath = Path.Combine(Application.dataPath, "Plugins", "ConiferInit");
            string testsPath = Path.Combine(pluginPath, "Scripts", "Tests");
            string[] filesWithoutTests = Directory.GetFileSystemEntries(pluginPath, "*", SearchOption.AllDirectories)
                .Where(e => !e.EndsWith(".meta")).Where(e => !e.Contains(testsPath))
                .Select(e => e.Replace(Application.dataPath, "Assets"))
                .ToArray();

            List<string> pathsToExport = new List<string>();
            pathsToExport.Add(Path.Combine("Assets", "Editor", "ICSharpCode.SharpZipLib.dll"));
            pathsToExport.AddRange(filesWithoutTests);
            
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
#endif
    }
}