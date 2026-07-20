using System.IO;
using ConiferInit.Editor.Configuration;
using ConiferInit.Editor.Execution;
using ConiferInit.Editor.UI;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor
{
    /// <summary>
    /// Coordinates draw logic and handling user inputs.
    /// </summary>
    internal sealed class Window : EditorWindow
    {
        internal readonly struct Dependencies
        {
            public SetupExecution Execution { get; init; }
            public Styles Styles { get; init; }
            
            public SetupConfiguration Configuration { get; init; }
            public SettingsProfileConfiguration SettingsProfileConfiguration { get; init; }
            public PackagesSettingsConfiguration PackagesSettingsConfiguration { get; init; }
            public AssetsSettingsConfiguration AssetsSettingsConfiguration { get; init; }

            public HeaderUI HeaderUI { get; init; }
            public ProfileSettingsUI ProfileSettingsUI { get; init; }
            public FolderStructureUI FolderStructureUI { get; init; }
            public PackagesSettingsUI PackagesSettingsUI { get; init; }
            public AssetsSettingsUI AssetsSettingsUI { get; init; }
            public ProjectSettingsUI ProjectSettingsUI { get; init; }
            public MiscSettingsUI MiscSettingsUI { get; init; }
            public ExecuteUI ExecuteUI { get; init; }
            public FooterUI FooterUI { get; init; }
        }

        private SetupExecution _execution;
        private Styles _styles;
        
        private SetupConfiguration _configuration;
        
        private SettingsProfileConfiguration _settingsProfileConfiguration;
        private PackagesSettingsConfiguration _packagesSettingsConfiguration;
        private AssetsSettingsConfiguration _assetsSettingsConfiguration;
        
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


        private void OnEnable()
        {
            CompositionRoot.ResolveWindowDependencies(this);
        }


        public void Construct(Dependencies dependencies)
        {
            _execution = dependencies.Execution;
            _styles = dependencies.Styles;
            
            _configuration = dependencies.Configuration;
            _settingsProfileConfiguration = dependencies.SettingsProfileConfiguration;
            _packagesSettingsConfiguration = dependencies.PackagesSettingsConfiguration;
            _assetsSettingsConfiguration = dependencies.AssetsSettingsConfiguration;

            _headerUI = dependencies.HeaderUI;
            _profileSettingsUI = dependencies.ProfileSettingsUI;
            _folderStructureUI = dependencies.FolderStructureUI;
            _packagesSettingsUI = dependencies.PackagesSettingsUI;
            _packagesSettingsUI = dependencies.PackagesSettingsUI;
            _assetsSettingsUI = dependencies.AssetsSettingsUI;
            _projectSettingsUI = dependencies.ProjectSettingsUI;
            _miscSettingsUI = dependencies.MiscSettingsUI;
            _executeUI = dependencies.ExecuteUI;
            _footerUI = dependencies.FooterUI;
            
            _settingsProfileConfiguration.ApplyingProfile += ResetTemporaryState;
            _headerUI.UninstallRequested += OnUninstallRequested;
        }


        public void Initialize()
        {
            _settingsProfileConfiguration.Initialize();
            _packagesSettingsConfiguration.Initialize();
            _assetsSettingsConfiguration.Initialize();
        }


        private void ResetTemporaryState()
        {
            _folderStructureUI.ResetTemporaryState();
            _packagesSettingsUI.ResetTemporaryState();
            _assetsSettingsUI.ResetTemporaryState();
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


        private void OnInspectorUpdate()
        {
            Repaint();
        }


        private void OnGUI()
        {
            if (!_styles.Initialized)
            {
                _styles.Initialize();
            }
            
            using GUILayout.ScrollViewScope scrollViewScope =
                new GUILayout.ScrollViewScope(_scrollPosition, _styles.EntireWindow);
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


        private void Update()
        {
            _configuration.Update();
            _execution.Update();
        }


        private void OnDisable()
        {
            _settingsProfileConfiguration.ApplyingProfile -= ResetTemporaryState;
            _headerUI.UninstallRequested -= OnUninstallRequested;
        }
    }
}