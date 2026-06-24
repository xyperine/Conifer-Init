using System;
using System.Collections.Generic;

namespace ProjectSetupTool.Editor.Configuration
{
    [Serializable]
    internal sealed class SettingsProfile
    {
        public string Name { get; set; }
        
        public FolderStructureEntry AssetsFolderStructureEntry { get; set; }
        public List<PackageImportEntry> QueuedPackages { get; set; }
        public List<AssetImportEntry> QueuedAssets { get; set; }
        public ProjectSettings ProjectSettings { get; set; }
        public MiscSettings MiscSettings { get; set; }
    }
}