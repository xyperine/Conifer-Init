using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor.UI
{
    /// <summary>
    /// Coordinates draw logic, handling user inputs, and setup execution. Also acts as a composition root.
    /// </summary>
    public class SetupWindow : EditorWindow
    {
        private readonly SetupConfiguration _configuration = new SetupConfiguration();

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
            SetupWindow window = GetWindow<SetupWindow>();
            window.titleContent = new GUIContent("Setup");
            window.Show();
        }


        private void OnEnable()
        {
            _configuration.Initialize();
            _configuration.ApplyingProfile += ResetTemporaryState;

            _profileSettingsUI = new ProfileSettingsUI(_configuration);
            _folderStructureUI = new FolderStructureUI(_configuration);
            _packagesSettingsUI = new PackagesSettingsUI(_configuration);
            _assetsSettingsUI = new AssetsSettingsUI(_configuration);
            _projectSettingsUI = new ProjectSettingsUI(_configuration);
            _miscSettingsUI = new MiscSettingsUI(_configuration);
        }


        private void OnGUI()
        {
            using GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope(_scrollPosition);
            _scrollPosition = scrollViewScope.scrollPosition;
            
            _profileSettingsUI.Draw();
            
            SetupWindowElements.DrawRegularSpace();
            
            _folderStructureUI.Draw();

            SetupWindowElements.DrawRegularSpace();

            _packagesSettingsUI.Draw();
            
            SetupWindowElements.DrawRegularSpace();
            
            _assetsSettingsUI.Draw();

            SetupWindowElements.DrawRegularSpace();
            
            _projectSettingsUI.Draw();
            
            SetupWindowElements.DrawRegularSpace();
            
            _miscSettingsUI.Draw();
            
            SetupWindowElements.DrawRegularSpace();
            
            DrawExecuteSetup();
            
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
            if (GUILayout.Button("Execute Setup", new GUIStyle(GUI.skin.button), GUILayout.Width(128f)))
            {
                _configuration.ExecuteSetup();
            }
        }
        
        
        private void Update()
        {
            _configuration.Update();
        }


        private void OnDisable()
        {
            _configuration.ApplyingProfile -= ResetTemporaryState;
        }
    }
}