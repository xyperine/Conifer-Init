namespace ConiferInit.Editor.Configuration
{
    internal sealed class FolderStructureConfiguration
    {
        private readonly SettingsProfileConfiguration _profileConfiguration;
        private readonly ConfigurationCache _configurationCache;


        public FolderStructureConfiguration(SettingsProfileConfiguration profileConfiguration, ConfigurationCache configurationCache)
        {
            _profileConfiguration = profileConfiguration;
            _configurationCache = configurationCache;
        }
        
        
        public FolderStructureEntry GetAssetsFolder()
        {
            return _configurationCache.AssetsFolderStructureEntry;
        }
        
        
        public void ResetFolderStructure()
        {
            _configurationCache.AssetsFolderStructureEntry =
                FolderStructureEntry.DeepCopy(_profileConfiguration.ActiveProfile.AssetsFolderStructureEntry, null);
        }


        public void RemoveFolder(FolderStructureEntry entry)
        { 
            entry.Parent.RemoveChild(entry);
        }


        public void RenameFolder(FolderStructureEntry entry, string newName)
        {
            entry.Rename(newName);
        }


        public void AddFolder(string folderName, FolderStructureEntry parent)
        {
            parent.AddChild(new FolderStructureEntry(folderName, parent));
        }
    }
}