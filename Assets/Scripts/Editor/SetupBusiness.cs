using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Assertions;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ProjectSetup.Editor
{
    // Silly name for now
    public class SetupBusiness
    {
        // Profiles
        public const string DEFAULT_PROFILE_NAME = "Default_Profile";
        
        private SettingsProfile _activeProfile;
        private List<SettingsProfile> _profiles;
        
        public SettingsProfile DefaultProfile => _profiles.Find(p => p.Name == DEFAULT_PROFILE_NAME);
        public List<SettingsProfile> Profiles => _profiles;

        public SettingsProfile ActiveProfile => _activeProfile;

        // Packages
        private bool _successfullyRetrievedPackages;
        private Dictionary<string, PackageInfo> _allPackages;

        public List<string> AvailablePackages => _successfullyRetrievedPackages && _allPackages != null
            ? _allPackages.Keys.Where(id =>
                !ProjectSetupData.instance.QueuedPackagesIDs.Contains(id)).ToList()
            : new List<string>();
        
        public SearchRequest PackagesListRequest { get; private set; }

        // Assets

        public bool SuccessfullyRetrievedAssets { get; private set; } = false;

        private readonly Dictionary<string, AssetInfo> _assets = new Dictionary<string, AssetInfo>();
        
        public List<string> AvailableAssets => SuccessfullyRetrievedAssets && _assets != null
            ? _assets.Keys.Where(id =>
                !ProjectSetupData.instance.QueuedAssets.Exists(a => a.ID == id)).ToList()
            : new List<string>();
        
        
        public void Initialize()
        {
            LoadSettingsProfiles();

            PackagesListRequest = Client.SearchAll();

            RetrieveCachedAssets();
        }

        
        public void LoadSettingsProfiles()
        {
            if (!Directory.Exists(PersistenceSerializer<SettingsProfile>.ProfilesStoragePath))
            {
                Directory.CreateDirectory(PersistenceSerializer<SettingsProfile>.ProfilesStoragePath);
            }
            
            IEnumerable<string> profilePaths =
                Directory.EnumerateFiles(PersistenceSerializer<SettingsProfile>.ProfilesStoragePath,
                    "*.json");
            _profiles = profilePaths
                .Select(pp => PersistenceSerializer<SettingsProfile>.ReadFile(Path.GetFileName(pp)))
                .ToList();
            
            string activeProfileName = ProjectSetupData.instance.ActiveSettingsProfileName;
            Debug.Log(activeProfileName);
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
                    QueuedPackagesIDs = new List<string>(),
                    QueuedAssets = new List<AssetImportEntry>(),
                    ProjectSettings = ProjectSettings.Default(),
                    MiscSettings = MiscSettings.Default(),
                };
                    
                PersistenceSerializer<SettingsProfile>.SaveFile(defaultProfile, defaultProfile.Name);
                
                _profiles.Add(defaultProfile);
            }
            
            ApplyProfile(DefaultProfile);
        }
        
        
        public void ApplyProfile(SettingsProfile profile)
        {
            _activeProfile = profile;
            ProjectSetupData.instance.ActiveSettingsProfileName = _activeProfile.Name;
            
            ProjectSetupData.instance.AssetsFolderStructureEntry = FolderStructureEntry.DeepCopy(_activeProfile.AssetsFolderStructureEntry, null);
            ProjectSetupData.instance.QueuedPackagesIDs = new List<string>(_activeProfile.QueuedPackagesIDs);
            ProjectSetupData.instance.QueuedAssets = new List<AssetImportEntry>(_activeProfile.QueuedAssets);
            ProjectSetupData.instance.ProjectSettings = _activeProfile.ProjectSettings;
            ProjectSetupData.instance.MiscSettings = _activeProfile.MiscSettings;
        }


        public void SaveProfile(SettingsProfile profile)
        {
            profile.AssetsFolderStructureEntry = ProjectSetupData.instance.AssetsFolderStructureEntry;
            profile.QueuedPackagesIDs = new List<string>(ProjectSetupData.instance.QueuedPackagesIDs);
            profile.QueuedAssets = new List<AssetImportEntry>(ProjectSetupData.instance.QueuedAssets);
            profile.ProjectSettings = ProjectSetupData.instance.ProjectSettings;
            profile.MiscSettings = ProjectSetupData.instance.MiscSettings;
            
            PersistenceSerializer<SettingsProfile>.SaveFile(profile, profile.Name);
            
            Debug.Log($"Saved {profile.Name} profile");
            
            LoadSettingsProfiles();
            
            ApplyProfile(profile);
        }


        public void DeleteProfile(SettingsProfile profile)
        {
            ApplyProfile(DefaultProfile);
            
            PersistenceSerializer<SettingsProfile>.DeleteFile(profile.Name);
            
            Debug.Log($"Deleted {profile.Name} profile");
            
            LoadSettingsProfiles();
            
            ApplyProfile(DefaultProfile);
        }


        public void TrySaveProfileAt(string path, Action<string> onSuccess)
        {
            // Extract all of this
            bool insideProfileStorage = Directory.GetParent(path).FullName ==
                                        PersistenceSerializer<SettingsProfile>.ProfilesStoragePath;
            bool hasRightExtension = Path.GetExtension(path) == ".json";
            bool isNotDefaultProfile = Path.GetFileNameWithoutExtension(path) != DEFAULT_PROFILE_NAME;
            bool valid = insideProfileStorage && hasRightExtension && isNotDefaultProfile;
            if (valid)
            {
                onSuccess?.Invoke(path);
            }
            else if (!insideProfileStorage)
            {
                Debug.LogError(
                    $"Must be inside the profiles storage directory!: {PersistenceSerializer<SettingsProfile>.ProfilesStoragePath}");
            }
            else if (!hasRightExtension)
            {
                Debug.LogError("The profile file must have .json extension!");
            }
            else if (!isNotDefaultProfile)
            {
                Debug.LogError("Can't override the default profile!");
            }

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


        public void ResetFolderStructure()
        {
            ProjectSetupData.instance.AssetsFolderStructureEntry =
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
            ProjectSetupData.instance.QueuedPackagesIDs.Add(id);
        }


        public void DequeuePackage(string id)
        {
            ProjectSetupData.instance.QueuedPackagesIDs.Remove(id);
        }


        /// <summary>
        /// Converts list of shorter package ids to a list of full package ids.
        /// </summary>
        /// <param name="packageIDs">List of shorter package ids, equivalent to PackageInfo.name.</param>
        /// <returns>List of full package ids, equivalent of PackageInfo.packageId.</returns>
        public IEnumerable<string> GetFullPackagesID(IEnumerable<string> packageIDs)
        {
            return packageIDs.Select(id => _allPackages[id].packageId);
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
            ProjectSetupData.instance.QueuedAssets.Add(new AssetImportEntry(_assets[id], false));
        }


        public void DequeueAsset(string id)
        {
            ProjectSetupData.instance.QueuedAssets.Remove(ProjectSetupData.instance.QueuedAssets.Find(a => a.ID == id));
        }


        public void SetInteractiveImportForAsset(string id, bool interactive)
        {
            int index = ProjectSetupData.instance.QueuedAssets.FindIndex(a => a.ID == id);
            AssetImportEntry entry = ProjectSetupData.instance.QueuedAssets[index];
            ProjectSetupData.instance.QueuedAssets[index] =
                new AssetImportEntry(entry.Path, entry.Name, entry.ID, interactive);
        }
    }
}