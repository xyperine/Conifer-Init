using ConiferInit.Editor.Configuration;
using ConiferInit.Editor.Execution;
using ConiferInit.Editor.UI;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor
{
    internal static class CompositionRoot
    {
        public static void ResolveWindowDependencies(Window window)
        {
            // Create business objects
            SetupExecution execution = new SetupExecution(ExecutionCache.instance, ConfigurationCache.instance);
            
            SettingsProfileConfiguration settingsProfileConfiguration = new SettingsProfileConfiguration(ConfigurationCache.instance);
            FolderStructureConfiguration folderStructureConfiguration =
                new FolderStructureConfiguration(settingsProfileConfiguration, ConfigurationCache.instance);
            PackagesSettingsConfiguration packagesSettingsConfiguration = new PackagesSettingsConfiguration(ConfigurationCache.instance);
            AssetsSettingsConfiguration assetsSettingsConfiguration = new AssetsSettingsConfiguration(ConfigurationCache.instance);
            ProjectSettingsConfiguration projectSettingsConfiguration = new ProjectSettingsConfiguration(ConfigurationCache.instance);
            MiscSettingsConfiguration miscSettingsConfiguration = new MiscSettingsConfiguration(ConfigurationCache.instance);
            SetupConfiguration configuration = new SetupConfiguration(ConfigurationCache.instance, ExecutionCache.instance,
                settingsProfileConfiguration, packagesSettingsConfiguration, assetsSettingsConfiguration);

            // Create UI
            Styles styles = new Styles();
            
            HeaderUI headerUI = new HeaderUI(configuration, execution, styles);
            
            ProfileSettingsUI profileSettingsUI = new ProfileSettingsUI(settingsProfileConfiguration, styles);
            FolderStructureUI folderStructureUI = new FolderStructureUI(folderStructureConfiguration, styles);
            PackagesSettingsUI packagesSettingsUI = new PackagesSettingsUI(packagesSettingsConfiguration, styles);
            AssetsSettingsUI assetsSettingsUI = new AssetsSettingsUI(assetsSettingsConfiguration, styles);
            ProjectSettingsUI projectSettingsUI = new ProjectSettingsUI(projectSettingsConfiguration, styles);
            MiscSettingsUI miscSettingsUI = new MiscSettingsUI(miscSettingsConfiguration, styles);
            
            ExecuteUI executeUI = new ExecuteUI(execution, styles);
            
            FooterUI footerUI = new FooterUI(styles);
            
            // Resolve window dependencies
            Window.Dependencies windowDependencies = new Window.Dependencies
            {
                Execution = execution,
                Styles = styles,
                AssetsSettingsUI = assetsSettingsUI,
                ProjectSettingsUI = projectSettingsUI,
                MiscSettingsUI = miscSettingsUI,
                ExecuteUI = executeUI,
                FooterUI = footerUI,
                Configuration = configuration,
                SettingsProfileConfiguration = settingsProfileConfiguration,
                HeaderUI = headerUI,
                ProfileSettingsUI = profileSettingsUI,
                FolderStructureUI = folderStructureUI,
                PackagesSettingsUI = packagesSettingsUI,
                PackagesSettingsConfiguration = packagesSettingsConfiguration,
                AssetsSettingsConfiguration = assetsSettingsConfiguration,
            };
            window.Construct(windowDependencies);
            window.Initialize();
        }


        public static Window CreateWindow()
        {
            Vector2 size = new Vector2(600f, 800f);
            int width = Screen.currentResolution.width;
            int height = Screen.currentResolution.height;
            Vector2 center = new Vector2((width - size.x) * 0.5f, (height - size.y) * 0.5f);
            Window window = EditorWindow.GetWindow<Window>(false, "Conifer Init", true);
            window.position = new Rect(center, size);
            window.minSize = size;
            window.Show();

            return window;
        }
    }
}