using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor.Configuration
{
    /// <summary>
    /// Contains tool configuration data that needs to survive domain reloads and persist across sessions.
    /// </summary>
    [FilePath("Library/ProjectSetupTool/ConfigurationCache.txt", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class SetupConfigurationCache : ScriptableSingleton<SetupConfigurationCache>
    {
        [SerializeField] private string activeSettingsProfileName;
        [SerializeField] private MiscSettings miscSettings;
        [SerializeField] private ProjectSettings projectSettings;
        [SerializeField] private List<AssetImportEntry> queuedAssets;
        [SerializeField] private List<PackageImportEntry> queuedPackages;
        [SerializeField] private FolderStructureEntry assetsFolderStructureEntry;
        
        public FolderStructureEntry AssetsFolderStructureEntry
        {
            get => assetsFolderStructureEntry;
            set => assetsFolderStructureEntry = value;
        }

        public List<PackageImportEntry> QueuedPackages
        {
            get => queuedPackages;
            set => queuedPackages = value;
        }
        
        public List<AssetImportEntry> QueuedAssets
        {
            get => queuedAssets;
            set => queuedAssets = value;
        }

        public ProjectSettings ProjectSettings
        {
            get => projectSettings;
            set => projectSettings = value;
        }

        public MiscSettings MiscSettings
        {
            get => miscSettings;
            set => miscSettings = value;
        }

        public string ActiveSettingsProfileName
        {
            get => activeSettingsProfileName;
            set => activeSettingsProfileName = value;
        }
        
        
        public void Save()
        {
            Save(true);
        }
    }
}