using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ConiferInit.Editor.Configuration
{
    internal sealed class SettingsProfileConfiguration
    {
        public const string DEFAULT_PROFILE_NAME = "Default_Profile";
        
        private readonly ConfigurationCache _configurationCache;
        
        private SettingsProfile _activeProfile;
        private List<SettingsProfile> _profiles;
        
        public SettingsProfile DefaultProfile => _profiles.Find(p => p.Name == DEFAULT_PROFILE_NAME);
        public List<SettingsProfile> Profiles => _profiles;

        public SettingsProfile ActiveProfile => _activeProfile;

        public event Action ApplyingProfile;
        public event Action AppliedProfile;


        public SettingsProfileConfiguration(ConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache;
        }


        public void Initialize()
        {
            LoadSettingsProfiles();
        }
        
        
        public void LoadSettingsProfiles()
        {
            if (!Directory.Exists(SettingsProfilePersistency.StoragePath))
            {
                Directory.CreateDirectory(SettingsProfilePersistency.StoragePath);
            }
            
            IEnumerable<string> profilePaths =
                Directory.EnumerateFiles(SettingsProfilePersistency.StoragePath,
                    "*.json");
            _profiles = profilePaths
                .Select(pp => SettingsProfilePersistency.Restore(Path.GetFileName(pp)))
                .ToList();
            
            string activeProfileName = _configurationCache.ActiveSettingsProfileName;
            if (!string.IsNullOrEmpty(activeProfileName))
            {
                var p = _profiles.Find(p => p.Name == activeProfileName);
                if (p != null)
                {
                    _activeProfile = p;
                }
                else
                {
                    LoadDefaultProfile();
                }
            }
            else
            {
                LoadDefaultProfile();
            }
        }


        public void LoadDefaultProfile()
        {
            if (DefaultProfile == null)
            {
                SettingsProfile defaultProfile = new SettingsProfile()
                {
                    Name = DEFAULT_PROFILE_NAME,
                    AssetsFolderStructureEntry = FolderStructureEntry.Default(),
                    QueuedPackages = new List<PackageImportEntry>(),
                    QueuedAssets = new List<AssetImportEntry>(),
                    ProjectSettings = ProjectSettings.Default(),
                    MiscSettings = MiscSettings.Default(),
                };
                    
                SettingsProfilePersistency.Save(defaultProfile);
                
                _profiles.Add(defaultProfile);
            }
            
            ApplyProfile(DefaultProfile);
        }
        
        
        public void ApplyProfile(SettingsProfile profile)
        {
            ApplyingProfile?.Invoke();
            
            _activeProfile = profile;
            _configurationCache.ActiveSettingsProfileName = _activeProfile.Name;
            
            _configurationCache.AssetsFolderStructureEntry = FolderStructureEntry.DeepCopy(_activeProfile.AssetsFolderStructureEntry, null);
            _configurationCache.QueuedPackages = new List<PackageImportEntry>(_activeProfile.QueuedPackages);
            _configurationCache.QueuedAssets = new List<AssetImportEntry>(_activeProfile.QueuedAssets);
            _configurationCache.ProjectSettings = _activeProfile.ProjectSettings;
            _configurationCache.MiscSettings = _activeProfile.MiscSettings;
            
            AppliedProfile?.Invoke();
        }


        public void SaveProfile(SettingsProfile profile)
        {
            profile.AssetsFolderStructureEntry = _configurationCache.AssetsFolderStructureEntry;
            profile.QueuedPackages = new List<PackageImportEntry>(_configurationCache.QueuedPackages);
            profile.QueuedAssets = new List<AssetImportEntry>(_configurationCache.QueuedAssets);
            profile.ProjectSettings = _configurationCache.ProjectSettings;
            profile.MiscSettings = _configurationCache.MiscSettings;
            
            SettingsProfilePersistency.Save(profile);
            
            LoadSettingsProfiles();
            
            ApplyProfile(profile);
        }


        public void DeleteProfile(SettingsProfile profile)
        {
            ApplyProfile(DefaultProfile);
            
            SettingsProfilePersistency.Delete(profile.Name);
            
            LoadSettingsProfiles();
            
            ApplyProfile(DefaultProfile);
        }


        public bool IsValidProfilePath(string path)
        {
            bool insideProfileStorage = Directory.GetParent(path).FullName == SettingsProfilePersistency.StoragePath;
            bool hasRightExtension = Path.GetExtension(path) == ".json";
            bool isNotDefaultProfile = Path.GetFileNameWithoutExtension(path) != DEFAULT_PROFILE_NAME;
            
            if (!insideProfileStorage)
            {
                Debug.LogError(
                    $"Must be inside the profiles storage directory!: {SettingsProfilePersistency.StoragePath}");
            }
            
            if (!hasRightExtension)
            {
                Debug.LogError("The profile file must have .json extension!");
            }
            
            if (!isNotDefaultProfile)
            {
                Debug.LogError("Can't override the default profile!");
            }

            bool valid = insideProfileStorage && hasRightExtension && isNotDefaultProfile;
            return valid;
        }


        public bool IsValidNewProfilePath(string path)
        {
            if (File.Exists(path))
            {
                Debug.LogError("Can't override profiles with the \"New\" option!");
                return false;
            }
            
            return IsValidProfilePath(path);
        }


        /// <summary>
        /// Creates a unique name for a new profile.
        /// </summary>
        /// <returns>Unique name that doesn't exist in the profiles storage.</returns>
        public string ConstructNewProfileName()
        {
            string newName = "New_Profile";
            if (Profiles.Exists(p => p.Name == newName))
            {
                int i = 1;
                while (Profiles.Any(p => p.Name == newName))
                {
                    newName = "New_Profile" + $"_{i}"; 
                    i++;
                }
            }

            return newName;
        }
    }
}