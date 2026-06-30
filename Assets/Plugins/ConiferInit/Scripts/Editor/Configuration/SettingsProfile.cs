using System;
using System.Collections.Generic;
using System.Linq;

namespace ConiferInit.Editor.Configuration
{
    [Serializable]
    internal sealed class SettingsProfile : IEquatable<SettingsProfile>
    {
        public string Name { get; set; }
        
        public FolderStructureEntry AssetsFolderStructureEntry { get; set; }
        public List<PackageImportEntry> QueuedPackages { get; set; }
        public List<AssetImportEntry> QueuedAssets { get; set; }
        public ProjectSettings ProjectSettings { get; set; }
        public MiscSettings MiscSettings { get; set; }


        public bool Equals(SettingsProfile other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            bool equalQueuedPackages = QueuedPackages.SequenceEqual(other.QueuedPackages);
            bool equalQueuedAssets = QueuedAssets.SequenceEqual(other.QueuedAssets);

            return Name == other.Name && Equals(AssetsFolderStructureEntry, other.AssetsFolderStructureEntry) &&
                   equalQueuedPackages && equalQueuedAssets && ProjectSettings.Equals(other.ProjectSettings) &&
                   MiscSettings.Equals(other.MiscSettings);
        }


        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SettingsProfile other && Equals(other);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(Name, AssetsFolderStructureEntry, QueuedPackages, QueuedAssets, ProjectSettings,
                MiscSettings);
        }
    }
}