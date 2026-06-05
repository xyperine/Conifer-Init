using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Contains all the tool data that needs to survive domain reloads and even persist across sessions.
    /// </summary>
    [FilePath("Library/ProjectSetupTool/Data.txt", FilePathAttribute.Location.ProjectFolder)]
    public class ProjectSetupData : ScriptableSingleton<ProjectSetupData>
    {
        [SerializeField] private string activeSettingsProfileName;
        [SerializeField] private MiscSettings miscSettings;
        [SerializeField] private ProjectSettings projectSettings;
        [SerializeField] private List<string> queuedAssetIDs;
        [SerializeField] private List<int> queuedPackagesIndices;
        [SerializeField] private FolderStructureEntry assetsFolderStructureEntry;
        
        public FolderStructureEntry AssetsFolderStructureEntry
        {
            get => assetsFolderStructureEntry;
            set
            {
                assetsFolderStructureEntry = value;
                Save(true);
            }
        }

        public List<int> QueuedPackagesIndices
        {
            get => queuedPackagesIndices;
            set
            {
                queuedPackagesIndices = value;
                Save(true);
            }
        }
        
        public List<string> QueuedAssetIDs
        {
            get => queuedAssetIDs;
            set
            {
                queuedAssetIDs = value;
                Save(true);
            }
        }

        public ProjectSettings ProjectSettings
        {
            get => projectSettings;
            set
            {
                projectSettings = value;
                Save(true);
            }
        }

        public MiscSettings MiscSettings
        {
            get => miscSettings;
            set
            {
                miscSettings = value;
                Save(true);
            }
        }

        public string ActiveSettingsProfileName
        {
            get => activeSettingsProfileName;
            set
            {
                activeSettingsProfileName = value;
                Save(true);
            }
        }
        
        // For sequential interactive assets import
        [field: SerializeField] public List<AssetInfo> AssetsToImport { get; set; }
        [field: SerializeField] public bool IsImporting { get; set; }
        [field: SerializeField] public bool IsStable { get; set; }
        [field: SerializeField] public bool IsImportRequested { get; set; }
    }
}