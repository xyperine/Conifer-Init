using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Coordinates draw logic, handling user inputs, and setup execution.
    /// </summary>
    public class SetupWindow : EditorWindow
    {
        private readonly SetupBusiness _business = new SetupBusiness();

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
            _business.Initialize();
            _business.ApplyingProfile += ResetTemporaryState;

            _profileSettingsUI = new ProfileSettingsUI(_business);
            _folderStructureUI = new FolderStructureUI(_business);
            _packagesSettingsUI = new PackagesSettingsUI(_business);
            _assetsSettingsUI = new AssetsSettingsUI(_business);
            _projectSettingsUI = new ProjectSettingsUI(_business);
            _miscSettingsUI = new MiscSettingsUI(_business);
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
                _business.ExecuteSetup();
            }
        }
        
        
        private void Update()
        {
            _business.Update();
        }


        private void OnDisable()
        {
            _business.ApplyingProfile -= ResetTemporaryState;
        }
    }
}