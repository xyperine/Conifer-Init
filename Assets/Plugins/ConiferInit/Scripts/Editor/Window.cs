using System.IO;
using System.Runtime.CompilerServices;
using ConiferInit.Editor.Configuration;
using ConiferInit.Editor.Execution;
using ConiferInit.Editor.UI;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("ConiferInit.Editor.Tests", AllInternalsVisible = true)]

namespace ConiferInit.Editor
{
    /// <summary>
    /// Coordinates draw logic, handling user inputs, and setup execution. Also acts as a composition root.
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
            using (new GUILayout.HorizontalScope(new GUIStyle()))
            {
                const float maxSize = 96f;
                GUILayout.FlexibleSpace();
                GUILayout.Label(_logo, GUILayout.MaxHeight(maxSize), GUILayout.MaxWidth(maxSize));
                GUILayout.Space(16f);
                GUILayout.Label("Conifer Init",
                    new GUIStyle(GUI.skin.label) {fontSize = 42, alignment = TextAnchor.MiddleLeft, font = _font},
                    GUILayout.Height(maxSize));
                GUILayout.FlexibleSpace();
            }
            
            WindowElements.DrawSectionSpace();

            DrawToolsOptions();
        }


        private void DrawToolsOptions()
        {
            using GUILayout.HorizontalScope s = new GUILayout.HorizontalScope(new GUIStyle());

            GUIStyle style = new GUIStyle(GUI.skin.button);
            
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset Configuration", style, GUILayout.Height(20f), GUILayout.Width(128f)))
            {
                ConfigurationCache.instance.Clear();
            }
            
            GUILayout.Space(16f);

            if (GUILayout.Button("Reset Execution", style, GUILayout.Height(20f), GUILayout.Width(128f)))
            {
                ExecutionCache.instance.Clear();
            }
            
            GUILayout.Space(16f);

            if (GUILayout.Button("Uninstall", style, GUILayout.Height(20f), GUILayout.Width(128f)))
            {
                bool wantToUninstall = EditorDialog.DisplayDecisionDialog("Uninstall?",
                    "Do you want to remove Conifer Init from your project?", "Yes", "No");
                if (wantToUninstall)
                {
                    Close();

                    string cachePath = Path.Combine(Application.dataPath.Replace("Assets", "Library"),
                        "ConiferInit");
                    Directory.Delete(cachePath, true);
                    AssetDatabase.DeleteAsset("Assets/Plugins/ConiferInit");
                }
            }
            
            GUILayout.FlexibleSpace();
        }


        private void DrawExecuteSetup()
        {
            using GUILayout.HorizontalScope s = new GUILayout.HorizontalScope(new GUIStyle());

            if (GUILayout.Button("Execute Setup", new GUIStyle(GUI.skin.button), GUILayout.Height(32f)))
            {
                _execution.ExecuteSetup();
            }
        }


        private void DrawFooter()
        {
            const string githubUrl = "https://github.com/xyperine";
            const string itchIoUrl = "https://xyperine.itch.io/";
            // DON'T FORGET TO UPDATE THIS WHENEVER THE VERSION CHANGES
            const string version = "v0.1.0";
            
            using var s = new GUILayout.HorizontalScope(new GUIStyle());
            
            if (EditorGUILayout.LinkButton("GitHub"))
            {
                Application.OpenURL(githubUrl);
            }

            WindowElements.DrawRegularSpace();
            
            if (EditorGUILayout.LinkButton("itch.io"))
            {
                Application.OpenURL(itchIoUrl);
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