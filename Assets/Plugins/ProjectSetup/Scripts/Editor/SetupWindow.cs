using System.Runtime.CompilerServices;
using ProjectSetupTool.Editor.Configuration;
using ProjectSetupTool.Editor.Execution;
using ProjectSetupTool.Editor.UI;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("Editor.Tests", AllInternalsVisible = true)]

namespace ProjectSetupTool.Editor
{
    /// <summary>
    /// Coordinates draw logic, handling user inputs, and setup execution. Also acts as a composition root.
    /// </summary>
    internal sealed class SetupWindow : EditorWindow
    {
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
        
        
        [MenuItem("Tools/Setup Window")]
        private static void ShowWindow()
        {
            Vector2 size = new Vector2(600f, 800f);
            int width = Screen.currentResolution.width;
            int height = Screen.currentResolution.height;
            Vector2 center = new Vector2((width - size.x) * 0.5f, (height - size.y) * 0.5f);
            SetupWindow window = GetWindow<SetupWindow>(false, "Setup", true);
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
            
            _profileSettingsUI.Draw();
            
            SetupWindowElements.DrawSectionSpace();
            
            _folderStructureUI.Draw();

            SetupWindowElements.DrawSectionSpace();

            _packagesSettingsUI.Draw();
            
            SetupWindowElements.DrawSectionSpace();
            
            _assetsSettingsUI.Draw();

            SetupWindowElements.DrawSectionSpace();
            
            _projectSettingsUI.Draw();
            
            SetupWindowElements.DrawSectionSpace();
            
            _miscSettingsUI.Draw();
            
            SetupWindowElements.DrawSectionSpace();
            
            DrawExecuteSetup();
            
            SetupWindowElements.DrawSectionSpace();
            SetupWindowElements.DrawHorizontalLine(1f);
            SetupWindowElements.DrawRegularSpace();
            
            DrawFooter();
            
            GUILayout.FlexibleSpace();
        }
        

        private void ResetTemporaryState()
        {
            _folderStructureUI.ResetTemporaryState();
            _packagesSettingsUI.ResetTemporaryState();
            _assetsSettingsUI.ResetTemporaryState();
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
            string version = PlayerSettings.bundleVersion;
            
            using var s = new GUILayout.HorizontalScope(new GUIStyle());
            
            if (EditorGUILayout.LinkButton("GitHub"))
            {
                Application.OpenURL(githubUrl);
            }

            SetupWindowElements.DrawRegularSpace();
            
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