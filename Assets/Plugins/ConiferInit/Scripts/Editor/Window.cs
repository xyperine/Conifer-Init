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
        private const string LOGO_PATH = "Assets/Plugins/ConiferInit/Textures/Logo.png";
        private const string TITLE_FONT_PATH = "Assets/Plugins/ConiferInit/Fonts/KodeMono-Regular.ttf";
        
        private readonly SetupConfiguration _configuration = new SetupConfiguration();
        private readonly SetupExecution _execution = new SetupExecution();

        private GUIStyle _entireWindowStyle;

        private ProfileSettingsUI _profileSettingsUI;
        private FolderStructureUI _folderStructureUI;
        private PackagesSettingsUI _packagesSettingsUI;
        private AssetsSettingsUI _assetsSettingsUI;
        private ProjectSettingsUI _projectSettingsUI;
        private MiscSettingsUI _miscSettingsUI;
        
        private Vector2 _scrollPosition;

        private GUIStyle _scopeStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _executeButtonStyle;

        private bool _stylesInitialized;

        private Texture2D _logo;
        private Font _font;
        
        
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
                {"LICENSE", "CHANGELOG.md", "THIRD_PARTY_NOTICES.txt", "User Guide.pdf"};
            
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
            
            _profileSettingsUI = new ProfileSettingsUI(_configuration);
            _folderStructureUI = new FolderStructureUI(_configuration);
            _packagesSettingsUI = new PackagesSettingsUI(_configuration);
            _assetsSettingsUI = new AssetsSettingsUI(_configuration);
            _projectSettingsUI = new ProjectSettingsUI(_configuration);
            _miscSettingsUI = new MiscSettingsUI(_configuration);
            
            _entireWindowStyle = new GUIStyle();
            _entireWindowStyle.padding = new RectOffset(16, 16, 16, 16);

            _logo = AssetDatabase.LoadAssetAtPath<Texture2D>(LOGO_PATH);
            _font = AssetDatabase.LoadAssetAtPath<Font>(TITLE_FONT_PATH);
        }


        private void OnInspectorUpdate()
        {
            Repaint();
        }

        
        private void OnGUI()
        {
            if (!_stylesInitialized)
            {
                _scopeStyle = new GUIStyle();
                _titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 42,
                    alignment = TextAnchor.MiddleLeft,
                    font = _font,
                };
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _executeButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 16,
                };

                _stylesInitialized = true;
            }
            
            using GUILayout.ScrollViewScope scrollViewScope =
                new GUILayout.ScrollViewScope(_scrollPosition, _entireWindowStyle);
            _scrollPosition = scrollViewScope.scrollPosition;
            
            DrawHeader();
            
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
            
            DrawExecuteSetup();
            
            WindowElements.DrawSectionSeparator();
            
            DrawFooter();
            
            GUILayout.FlexibleSpace();
        }
        

        private void ResetTemporaryState()
        {
            _folderStructureUI.ResetTemporaryState();
            _packagesSettingsUI.ResetTemporaryState();
            _assetsSettingsUI.ResetTemporaryState();
        }


        private void DrawHeader()
        {
            using (new GUILayout.HorizontalScope(_scopeStyle))
            {
                const float maxSize = 96f;
                GUILayout.FlexibleSpace();
                GUILayout.Label(_logo, GUILayout.MaxHeight(maxSize), GUILayout.MaxWidth(maxSize));
                GUILayout.Space(16f);
                GUILayout.Label("Conifer Init", _titleStyle, GUILayout.Height(maxSize));
                GUILayout.FlexibleSpace();
            }
            
            WindowElements.DrawSectionSpace();

            DrawToolsOptions();
        }


        private void DrawToolsOptions()
        {
            using GUILayout.HorizontalScope s = new GUILayout.HorizontalScope(_scopeStyle);
            
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset Configuration", _buttonStyle, GUILayout.Height(20f), GUILayout.Width(128f)))
            {
                ConfigurationCache.instance.Clear();
            }
            
            GUILayout.Space(16f);

            if (GUILayout.Button("Reset Execution", _buttonStyle, GUILayout.Height(20f), GUILayout.Width(128f)))
            {
                ExecutionCache.instance.Clear();
            }
            
            GUILayout.Space(16f);

            if (GUILayout.Button("Uninstall", _buttonStyle, GUILayout.Height(20f), GUILayout.Width(128f)))
            {
#if UNITY_6000_3_OR_NEWER
                bool wantToUninstall = EditorDialog.DisplayDecisionDialog("Uninstall?",
                    "Do you want to remove Conifer Init from your project?", "Yes", "No");
#else
                bool wantToUninstall = EditorUtility.DisplayDialog("Uninstall?",
                    "Do you want to remove Conifer Init from your project?", "Yes", "No");
#endif
                if (wantToUninstall)
                {
                    Close();

                    string cachePath = Path.Combine(Application.dataPath.Replace("Assets", "Library"),
                        "ConiferInit");
                    Directory.Delete(cachePath, true);
                    AssetDatabase.DeleteAsset("Assets/Plugins/ConiferInit");
                    AssetDatabase.DeleteAsset("Assets/Editor/ICSharpCode.SharpZipLib.dll");
                }
            }
            
            GUILayout.FlexibleSpace();
        }


        private void DrawExecuteSetup()
        {
            using GUILayout.HorizontalScope s = new GUILayout.HorizontalScope(_scopeStyle);
            
            if (GUILayout.Button("Execute Setup", _executeButtonStyle, GUILayout.Height(32f)))
            {
                _execution.ExecuteSetup();
            }
        }


        private void DrawFooter()
        {
            const string githubUrl = "https://github.com/xyperine";
            const string itchIoUrl = "https://xyperine.itch.io/";
            const string sourceCodeUrl = "https://github.com/xyperine/Conifer-Init";
            const string userGuidedUrl = "https://github.com/xyperine/Conifer-Init/blob/main/User Guide.pdf";
            const string userGuideLocalPath = "Assets/Plugins/ConiferInit/User Guide.pdf";
            // DON'T FORGET TO UPDATE THIS WHENEVER THE VERSION CHANGES
            const string version = "0.1.0";
            
            using var s = new GUILayout.HorizontalScope(_scopeStyle);
            
            if (EditorGUILayout.LinkButton("GitHub"))
            {
                Application.OpenURL(githubUrl);
            }

            WindowElements.DrawRegularSpace();
            
            if (EditorGUILayout.LinkButton("itch.io"))
            {
                Application.OpenURL(itchIoUrl);
            }
            
            WindowElements.DrawRegularSpace();
            
            if (EditorGUILayout.LinkButton("Source Code"))
            {
                Application.OpenURL(sourceCodeUrl);
            }
            
            WindowElements.DrawRegularSpace();
            
            if (EditorGUILayout.LinkButton("User Guide"))
            {
                if (AssetDatabase.AssetPathExists(userGuideLocalPath))
                {
                    string userGuideFullPath = Path.Combine(System.Environment.CurrentDirectory, userGuideLocalPath);
                    Process.Start(userGuideFullPath);
                }
                else
                {
                    Application.OpenURL(userGuidedUrl);
                }
            }
            
            GUILayout.FlexibleSpace();
            
            GUILayout.Label(version);
        }
        
        
        private void Update()
        {
            _configuration.Update();
            _execution.Update();
        }


        private void OnDisable()
        {
            _configuration.ApplyingProfile -= ResetTemporaryState;
        }
    }
}