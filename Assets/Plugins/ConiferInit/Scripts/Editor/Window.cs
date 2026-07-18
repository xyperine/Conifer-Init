using System.IO;
using ConiferInit.Editor.Configuration;
using ConiferInit.Editor.Execution;
using ConiferInit.Editor.UI;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor
{
    /// <summary>
    /// Coordinates draw logic and handling user inputs. Also acts as a composition root.
    /// </summary>
    internal sealed class Window : EditorWindow
    {
        private readonly SetupExecution _execution = new SetupExecution();
        private readonly Styles _styles = new Styles();
        
        private SetupConfiguration _configuration;
        
        private SettingsProfileConfiguration _settingsProfileConfiguration;
        private FolderStructureConfiguration _folderStructureConfiguration;
        private PackagesSettingsConfiguration _packagesSettingsConfiguration;
        private AssetsSettingsConfiguration _assetsSettingsConfiguration;
        private ProjectSettingsConfiguration _projectSettingsConfiguration;
        private MiscSettingsConfiguration _miscSettingsConfiguration;
        
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
            InitializeConfiguration();
            
            InitializeUI();

            _execution.Initialize();
        }


        private void InitializeConfiguration()
        {
            _settingsProfileConfiguration = new SettingsProfileConfiguration(ConfigurationCache.instance);
            _settingsProfileConfiguration.Initialize();
            _settingsProfileConfiguration.ApplyingProfile += ResetTemporaryState;

            _folderStructureConfiguration =
                new FolderStructureConfiguration(_settingsProfileConfiguration, ConfigurationCache.instance);

            _packagesSettingsConfiguration = new PackagesSettingsConfiguration(ConfigurationCache.instance);
            _packagesSettingsConfiguration.Initialize();

            _assetsSettingsConfiguration = new AssetsSettingsConfiguration(ConfigurationCache.instance);
            _assetsSettingsConfiguration.Initialize();

            _projectSettingsConfiguration = new ProjectSettingsConfiguration(ConfigurationCache.instance);

            _miscSettingsConfiguration = new MiscSettingsConfiguration(ConfigurationCache.instance);
            
            _configuration = new SetupConfiguration(ConfigurationCache.instance, ExecutionCache.instance,
                _settingsProfileConfiguration, _packagesSettingsConfiguration, _assetsSettingsConfiguration);
            _configuration.Initialize();
        }


        private void InitializeUI()
        {
            _headerUI = new HeaderUI(_configuration, _execution, _styles);
            _headerUI.UninstallRequested += OnUninstallRequested;
            
            _profileSettingsUI = new ProfileSettingsUI(_settingsProfileConfiguration, _styles);
            _folderStructureUI = new FolderStructureUI(_folderStructureConfiguration, _styles);
            _packagesSettingsUI = new PackagesSettingsUI(_packagesSettingsConfiguration, _styles);
            _assetsSettingsUI = new AssetsSettingsUI(_assetsSettingsConfiguration, _styles);
            _projectSettingsUI = new ProjectSettingsUI(_projectSettingsConfiguration, _styles);
            _miscSettingsUI = new MiscSettingsUI(_miscSettingsConfiguration, _styles);
            
            _executeUI = new ExecuteUI(_execution, _styles);
            
            _footerUI = new FooterUI(_styles);
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
            _settingsProfileConfiguration.ApplyingProfile -= ResetTemporaryState;
            _headerUI.UninstallRequested -= OnUninstallRequested;
        }
    }
}