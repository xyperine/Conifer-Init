using System;
using System.Collections.Generic;

namespace ProjectSetup.Editor
{
    [Serializable]
    internal sealed class SettingsProfile
    {
        public string Name { get; set; }
        
        public FolderStructureEntry AssetsFolderStructureEntry { get; set; }
        public List<string> QueuedPackagesIDs { get; set; }
        public List<AssetImportEntry> QueuedAssets { get; set; }
        public ProjectSettings ProjectSettings { get; set; }
        public MiscSettings MiscSettings { get; set; }
    }
}