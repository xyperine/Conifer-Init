using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Contains all the tool data that needs to survive domain reloads.
    /// </summary>
    public class ProjectSetupData : ScriptableSingleton<ProjectSetupData>
    {
        [field: SerializeField] public List<AssetInfo> AssetsToImport { get; set; }
        [field: SerializeField] public bool IsImporting { get; set; }
        [field: SerializeField] public bool IsStable { get; set; }
        [field: SerializeField] public bool IsImportRequested { get; set; }
        [field: SerializeField] public FolderStructureEntry AssetsFolderStructureEntry { get; set; }
        [field: SerializeField] public List<int> QueuedPackagesIndices { get; set; }
        [field: SerializeField] public List<int> QueuedAssetIndices { get; set; }
        [field: SerializeField] public ProjectSettings ProjectSettings { get; set; }
        // misc settings
    }
}