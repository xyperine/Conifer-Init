using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConiferInit.Editor.Execution;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Assertions;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ConiferInit.Editor.Configuration
{
    /// <summary>
    /// Handles the configuration of the setup and setup execution flow.
    /// </summary>
    internal sealed class SetupConfiguration
    {
        private ConfigurationCache _configurationCache;
        private ExecutionCache _executionCache;
        
        // Profiles
        public const string DEFAULT_PROFILE_NAME = "Default_Profile";
        
        private SettingsProfile _activeProfile;
        private List<SettingsProfile> _profiles;
        
        public SettingsProfile DefaultProfile => _profiles.Find(p => p.Name == DEFAULT_PROFILE_NAME);
        public List<SettingsProfile> Profiles => _profiles;

        public SettingsProfile ActiveProfile => _activeProfile;

        public event Action ApplyingProfile;

        // Packages
        private bool _successfullyRetrievedPackages;
        private Dictionary<string, PackageInfo> _allPackages;

        public List<string> AvailablePackages { get; private set; }
        
        public SearchRequest PackagesListRequest { get; private set; }

        // Assets
        public bool SuccessfullyRetrievedAssets { get; private set; } = false;

        private readonly Dictionary<string, AssetInfo> _assets = new Dictionary<string, AssetInfo>();
        
        public List<string> AvailableAssets { get; private set; }
        
        
        public void Initialize()
        {
            _configurationCache = ConfigurationCache.instance;
            _executionCache = ExecutionCache.instance;
            
            LoadSettingsProfiles();

            PackagesListRequest = Client.SearchAll();

            RetrieveCachedAssets();
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
            
            GenerateAvailablePackages();
            GenerateAvailableAssets();
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


        public FolderStructureEntry GetAssetsFSE()
        {
            return _configurationCache.AssetsFolderStructureEntry;
        }
        
        
        public void ResetFolderStructure()
        {
            _configurationCache.AssetsFolderStructureEntry =
                FolderStructureEntry.DeepCopy(ActiveProfile.AssetsFolderStructureEntry, null);
        }


        public void RemoveFolderStructureEntry(FolderStructureEntry entry)
        { 
            entry.Parent.RemoveChild(entry);
        }


        public void RenameFolderStructureEntry(FolderStructureEntry entry, string newName)
        {
            entry.Rename(newName);
        }


        public void AddFolder(string folderName, FolderStructureEntry parent)
        {
            parent.AddChild(new FolderStructureEntry(folderName, parent));
        }


        public List<PackageImportEntry> GetQueuedPackageIDs()
        {
            return _configurationCache.QueuedPackages;
        }
        
        
        public bool SuccessfullyRetrievedPackages()
        {
            if (_successfullyRetrievedPackages)
            {
                return _allPackages != null;
            }
            
            switch (PackagesListRequest.Status)
            {
                case StatusCode.InProgress:
                    _successfullyRetrievedPackages = false;
                    break;
                case StatusCode.Success:
                    _successfullyRetrievedPackages = true;
                    _allPackages = PackagesListRequest.Result.ToDictionary(p => p.name, p => p);
                    GenerateAvailablePackages();
                    break;
                case StatusCode.Failure:
                    _successfullyRetrievedPackages = false;
                    break;
                default:
                    _successfullyRetrievedPackages = false;
                    throw new ArgumentOutOfRangeException();
            }

            return _successfullyRetrievedPackages;
        }


        private void GenerateAvailablePackages()
        {
            AvailablePackages = _successfullyRetrievedPackages && _allPackages != null
                ? _allPackages.Keys.Where(id =>
                    !_configurationCache.QueuedPackages.Exists(p => p.ShortID == id)).ToList()
                : new List<string>();
        }


        public List<string> FindPackages(string nameFilter)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(nameFilter));

            return AvailablePackages.FindAll(id => _allPackages[id].displayName.Contains(nameFilter,
                StringComparison.OrdinalIgnoreCase));
        }


        public PackageInfo GetPackageByID(string id)
        {
            return _allPackages[id];
        }


        public void QueuePackage(string id)
        {
            _configurationCache.QueuedPackages.Add(new PackageImportEntry(_allPackages[id]));

            GenerateAvailablePackages();
        }


        public void DequeuePackage(string id)
        {
            _configurationCache.QueuedPackages.Remove(_configurationCache.QueuedPackages.Find(p => p.ShortID == id));
            
            GenerateAvailablePackages();
        }
        
        
        public void RetrieveCachedAssets()
        {
            string cachedAssetsPath;
            if (Environment.OSVersion.Platform is PlatformID.MacOSX or PlatformID.Unix)
            {
                string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                cachedAssetsPath = Path.Combine(homeDirectory, "Library", "Unity", "Asset Store-5.x");
            }
            else
            {
                string defaultPath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unity");
                cachedAssetsPath = Path.Combine(EditorPrefs.GetString("AssetStoreCacheRootPath", defaultPath),
                    "Asset Store-5.x");
            }
                
            if (Directory.Exists(cachedAssetsPath))
            {
                string[] assetPaths = Directory.GetFiles(cachedAssetsPath, "*.unitypackage", SearchOption.AllDirectories);

                _assets.Clear();
                foreach (string assetPath in assetPaths)
                {
                    string id = Path.GetFileNameWithoutExtension(assetPath);
                    _assets.Add(id, new AssetInfo(assetPath, Path.GetFileNameWithoutExtension(assetPath), id));
                }

                SuccessfullyRetrievedAssets = true;
            }
            else
            {
                throw new DirectoryNotFoundException($"Couldn't find {cachedAssetsPath}");
            }
            
            GenerateAvailableAssets();
        }


        private void GenerateAvailableAssets()
        {
            AvailableAssets = SuccessfullyRetrievedAssets && _assets != null
                ? _assets.Keys.Where(id =>
                    !_configurationCache.QueuedAssets.Exists(a => a.ID == id)).ToList()
                : new List<string>();
        }


        public List<AssetImportEntry> GetQueuedAssets()
        {
            return _configurationCache.QueuedAssets;
        }
        
        
        public List<string> FindAssets(string nameFilter)
        {
            return AvailableAssets.FindAll(id =>
                _assets[id].Name.Contains(nameFilter,
                    StringComparison.OrdinalIgnoreCase));
        }


        public AssetInfo FindAssetByID(string id)
        {
            return _assets[id];
        }


        public void QueueAsset(string id)
        {
            _configurationCache.QueuedAssets.Add(new AssetImportEntry(_assets[id], false));
            
            GenerateAvailableAssets();
        }


        public void DequeueAsset(string id)
        {
            _configurationCache.QueuedAssets.Remove(_configurationCache.QueuedAssets.Find(a => a.ID == id));
            
            GenerateAvailableAssets();
        }


        public void SetInteractiveImportForAsset(string id, bool interactive)
        {
            int index = _configurationCache.QueuedAssets.FindIndex(a => a.ID == id);
            AssetImportEntry entry = _configurationCache.QueuedAssets[index];
            _configurationCache.QueuedAssets[index] =
                new AssetImportEntry(entry.Path, entry.Name, entry.ID, interactive);
        }


        public ProjectSettings GetProjectSettings()
        {
            return _configurationCache.ProjectSettings;
        }


        public void SetProjectSettings(ProjectSettings projectSettings)
        {
            _configurationCache.ProjectSettings = projectSettings;
        }


        public MiscSettings GetMiscSettings()
        {
            return _configurationCache.MiscSettings;
        }


        public void SetMiscSettings(MiscSettings miscSettings)
        {
            _configurationCache.MiscSettings = miscSettings;
        }


        public void ClearCache()
        {
            _configurationCache.Clear();
        }
        

        public void Update()
        {
            if (!_executionCache.SetupInProgress)
            {
                _configurationCache.Save();
            }
        }
    }
}