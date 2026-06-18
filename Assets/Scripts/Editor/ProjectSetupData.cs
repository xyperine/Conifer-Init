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
        [SerializeField] private List<AssetImportEntry> queuedAssets;
        [SerializeField] private List<string> queuedPackagesIDs;
        [SerializeField] private FolderStructureEntry assetsFolderStructureEntry;
        
        public FolderStructureEntry AssetsFolderStructureEntry
        {
            get => assetsFolderStructureEntry;
            set => assetsFolderStructureEntry = value;
        }

        public List<string> QueuedPackagesIDs
        {
            get => queuedPackagesIDs;
            set => queuedPackagesIDs = value;
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
        
        [field: SerializeField] public bool PreInteractiveOperationsInProgress { get; set; }
        [field: SerializeField] public bool PreInteractiveOperationsFinished { get; set; }
        
        [field: SerializeField] public bool InteractiveOperationsInProgress { get; set; }
        [field: SerializeField] public List<AssetImportEntry> AssetsToImport { get; set; }
        [field: SerializeField] public bool SetupInProgress { get; set; }
        [field: SerializeField] public bool Importing { get; set; }
        [field: SerializeField] public bool Stable { get; set; }
        [field: SerializeField] public bool ImportRequested { get; set; }
        [field: SerializeField] public bool InteractiveOperationsFinished { get; set; }
        
        [field: SerializeField] public bool NonInteractiveOperationsInProgress { get; set; }


        public void Save()
        {
            Save(true);
        }
    }
}