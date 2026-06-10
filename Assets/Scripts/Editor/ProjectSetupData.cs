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
        [SerializeField] private List<string> queuedPackagesIDs;
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

        public List<string> QueuedPackagesIDs
        {
            get => queuedPackagesIDs;
            set
            {
                queuedPackagesIDs = value;
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
        
        [field: SerializeField] public bool PreInteractiveOperationsInProgress { get; set; }
        [field: SerializeField] public bool PreInteractiveOperationsFinished { get; set; }
        
        [field: SerializeField] public bool InteractiveOperationsInProgress { get; set; }
        [field: SerializeField] public List<AssetInfo> AssetsToImport { get; set; }
        [field: SerializeField] public bool SetupInProgress { get; set; }
        [field: SerializeField] public bool Importing { get; set; }
        [field: SerializeField] public bool Stable { get; set; }
        [field: SerializeField] public bool ImportRequested { get; set; }
        [field: SerializeField] public bool InteractiveOperationsFinished { get; set; }
        
        [field: SerializeField] public bool NonInteractiveOperationsInProgress { get; set; }
    }
}