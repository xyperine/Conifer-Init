using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConiferInit.Editor.Configuration;
using ConiferInit.Editor.Execution;
using ConiferInit.Editor.UI;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ConiferInit.Editor
{
    /// <summary>
    /// Coordinates draw logic and handling user inputs. Also acts as a composition root.
    /// </summary>
    internal sealed class Window : EditorWindow
    {
        private readonly SetupConfiguration _configuration = new SetupConfiguration();
        private readonly SetupExecution _execution = new SetupExecution();

        private GUIStyle _entireWindowStyle;

        private HeaderUI _headerUI;
        private ProfileSettingsUI _profileSettingsUI;
        private FolderStructureUI _folderStructureUI;
        private PackagesSettingsUI _packagesSettingsUI;
        private AssetsSettingsUI _assetsSettingsUI;
        private ProjectSettingsUI _projectSettingsUI;
        private MiscSettingsUI _miscSettingsUI;
        private ExecuteUI _executeUI;
        private FooterUI _footerUI;
        
        private Vector2 _scrollPosition;
        
        
        [MenuItem("Tools/Conifer Init")]
        private static void ShowWindow()
        {
            Vector2 size = new Vector2(600f, 800f);
            int width = Screen.currentResolution.width;
            int height = Screen.currentResolution.height;
            Vector2 center = new Vector2((width - size.x) * 0.5f, (height - size.y) * 0.5f);
            Window window = GetWindow<Window>(false, "Conifer Init", true);
            window.position = new Rect(center, size);
            window.minSize = size;
            window.Show();
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


        private void OnEnable()
        {
            _configuration.Initialize();
            _configuration.ApplyingProfile += ResetTemporaryState;
            
            _execution.Initialize();

            _headerUI = new HeaderUI(_configuration, _execution);
            _headerUI.UninstallRequested += OnUninstallRequested;
            _profileSettingsUI = new ProfileSettingsUI(_configuration);
            _folderStructureUI = new FolderStructureUI(_configuration);
            _packagesSettingsUI = new PackagesSettingsUI(_configuration);
            _assetsSettingsUI = new AssetsSettingsUI(_configuration);
            _projectSettingsUI = new ProjectSettingsUI(_configuration);
            _miscSettingsUI = new MiscSettingsUI(_configuration);
            _executeUI = new ExecuteUI(_execution);
            _footerUI = new FooterUI();
            
            _entireWindowStyle = new GUIStyle();
            _entireWindowStyle.padding = new RectOffset(16, 16, 16, 16);
        }


        private void OnInspectorUpdate()
        {
            Repaint();
        }


        private void OnUninstallRequested()
        {
            Close();

            string cachePath = Path.Combine(Application.dataPath.Replace("Assets", "Library"),
                "ConiferInit");
            Directory.Delete(cachePath, true);
            AssetDatabase.DeleteAsset("Assets/Plugins/ConiferInit");
            AssetDatabase.DeleteAsset("Assets/Editor/ICSharpCode.SharpZipLib.dll");
        }

        
        private void OnGUI()
        {
            using GUILayout.ScrollViewScope scrollViewScope =
                new GUILayout.ScrollViewScope(_scrollPosition, _entireWindowStyle);
            _scrollPosition = scrollViewScope.scrollPosition;
            
            _headerUI.Draw();
            
            WindowElements.DrawSectionSeparator();
            
            _profileSettingsUI.Draw();
            
            WindowElements.DrawSectionSpace();
            
            _folderStructureUI.Draw();

            WindowElements.DrawSectionSpace();

            _packagesSettingsUI.Draw();
            
            WindowElements.DrawSectionSpace();
            
            _assetsSettingsUI.Draw();

            WindowElements.DrawSectionSpace();
            
            _projectSettingsUI.Draw();
            
            WindowElements.DrawSectionSpace();
            
            _miscSettingsUI.Draw();
            
            WindowElements.DrawSectionSpace();
            
            _executeUI.Draw();
            
            WindowElements.DrawSectionSeparator();
            
            _footerUI.Draw();
            
            GUILayout.FlexibleSpace();
        }
        

        private void ResetTemporaryState()
        {
            _folderStructureUI.ResetTemporaryState();
            _packagesSettingsUI.ResetTemporaryState();
            _assetsSettingsUI.ResetTemporaryState();
        }
        
        
        private void Update()
        {
            _configuration.Update();
            _execution.Update();
        }


        private void OnDisable()
        {
            _configuration.ApplyingProfile -= ResetTemporaryState;
            _headerUI.UninstallRequested -= OnUninstallRequested;
        }
    }
}