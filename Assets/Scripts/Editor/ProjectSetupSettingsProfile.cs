using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSetup.Editor
{
    [Serializable]
    public class ProjectSetupSettingsProfile
    {
        [field: SerializeField] public string Name { get; set; }
        
        [field: SerializeField] public FolderStructureEntry AssetsFolderStructureEntry { get; set; }
        [field: SerializeField] public List<int> QueuedPackagesIndices { get; set; }
        [field: SerializeField] public List<int> QueuedAssetIndices { get; set; }
        [field: SerializeField] public ProjectSettings ProjectSettings { get; set; }
    }
}