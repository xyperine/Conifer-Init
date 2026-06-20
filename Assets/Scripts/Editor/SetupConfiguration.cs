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
    /// <summary>
    /// Handles the configuration of the setup and setup execution flow.
    /// </summary>
    internal sealed class SetupConfiguration
    {
        private ProjectSetupData _data;
        
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
            _data = ProjectSetupData.instance;
            
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
            
            string activeProfileName = _data.ActiveSettingsProfileName;
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
            ApplyingProfile?.Invoke();
            
            _activeProfile = profile;
            _data.ActiveSettingsProfileName = _activeProfile.Name;
            
            _data.AssetsFolderStructureEntry = FolderStructureEntry.DeepCopy(_activeProfile.AssetsFolderStructureEntry, null);
            _data.QueuedPackagesIDs = new List<string>(_activeProfile.QueuedPackagesIDs);
            _data.QueuedAssets = new List<AssetImportEntry>(_activeProfile.QueuedAssets);
            _data.ProjectSettings = _activeProfile.ProjectSettings;
            _data.MiscSettings = _activeProfile.MiscSettings;
        }


        public void SaveProfile(SettingsProfile profile)
        {
            profile.AssetsFolderStructureEntry = _data.AssetsFolderStructureEntry;
            profile.QueuedPackagesIDs = new List<string>(_data.QueuedPackagesIDs);
            profile.QueuedAssets = new List<AssetImportEntry>(_data.QueuedAssets);
            profile.ProjectSettings = _data.ProjectSettings;
            profile.MiscSettings = _data.MiscSettings;
            
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


        public FolderStructureEntry GetAssetsFSE()
        {
            return _data.AssetsFolderStructureEntry;
        }
        
        
        public void ResetFolderStructure()
        {
            _data.AssetsFolderStructureEntry =
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


        public List<string> GetQueuedPackageIDs()
        {
            return _data.QueuedPackagesIDs;
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
                    !_data.QueuedPackagesIDs.Contains(id)).ToList()
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
            _data.QueuedPackagesIDs.Add(id);

            GenerateAvailablePackages();
        }


        public void DequeuePackage(string id)
        {
            _data.QueuedPackagesIDs.Remove(id);
            
            GenerateAvailablePackages();
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
            
            GenerateAvailableAssets();
        }


        private void GenerateAvailableAssets()
        {
            AvailableAssets = SuccessfullyRetrievedAssets && _assets != null
                ? _assets.Keys.Where(id =>
                    !_data.QueuedAssets.Exists(a => a.ID == id)).ToList()
                : new List<string>();
        }


        public List<AssetImportEntry> GetQueuedAssets()
        {
            return _data.QueuedAssets;
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
            _data.QueuedAssets.Add(new AssetImportEntry(_assets[id], false));
            
            GenerateAvailableAssets();
        }


        public void DequeueAsset(string id)
        {
            _data.QueuedAssets.Remove(_data.QueuedAssets.Find(a => a.ID == id));
            
            GenerateAvailableAssets();
        }


        public void SetInteractiveImportForAsset(string id, bool interactive)
        {
            int index = _data.QueuedAssets.FindIndex(a => a.ID == id);
            AssetImportEntry entry = _data.QueuedAssets[index];
            _data.QueuedAssets[index] =
                new AssetImportEntry(entry.Path, entry.Name, entry.ID, interactive);
        }


        public ProjectSettings GetProjectSettings()
        {
            return _data.ProjectSettings;
        }


        public void SetProjectSettings(ProjectSettings projectSettings)
        {
            _data.ProjectSettings = projectSettings;
        }


        public MiscSettings GetMiscSettings()
        {
            return _data.MiscSettings;
        }


        public void SetMiscSettings(MiscSettings miscSettings)
        {
            _data.MiscSettings = miscSettings;
        }


        public void ExecuteSetup()
        {
            _data.SetupInProgress = true;
        }


        public void Update()
        {
            _data.Save();
            
            if (_data.SetupInProgress)
            {
                PerformSetup();
            }
        }
        
        
        // Move this to SetupExecution?
        private void PerformSetup()
        {
            if (!_data.SetupInProgress)
            {
                return;
            }

            if (!_data.PreInteractiveOperationsInProgress)
            {
                _data.PreInteractiveOperationsInProgress = true;
                
                string[] folders = _data.AssetsFolderStructureEntry.ToFolderNames();
                SetupExecution.CreateFolders(folders);

                _data.PreInteractiveOperationsFinished = true;
            }

            if (!_data.InteractiveOperationsInProgress)
            {
                _data.InteractiveOperationsInProgress = true;
                
                Debug.Log("Starting interactive operations...");

                IEnumerable<AssetImportEntry> assets = _data.QueuedAssets.Where(a => a.Interactive);
                SetupExecution.ImportAssetsInteractive(assets);
            }
                
            if (!_data.NonInteractiveOperationsInProgress && _data.InteractiveOperationsFinished)
            {
                _data.NonInteractiveOperationsInProgress = true;
                
                Debug.Log("Starting non-interactive operations...");

                IEnumerable<AssetImportEntry> assets =
                    _data.QueuedAssets.Where(a => !a.Interactive);
                SetupExecution.ImportAssetsNonInteractive(assets);

                IEnumerable<string> packages = GetFullPackagesID(_data.QueuedPackagesIDs);
                SetupExecution.ImportPackages(packages);
                    
                SetupExecution.SetProjectSettings(_data.ProjectSettings);
                    
                SetupExecution.ExecuteMisc(_data.MiscSettings);

                _data.SetupInProgress = false;
                _data.InteractiveOperationsInProgress = false;
                _data.InteractiveOperationsFinished = false;
                _data.NonInteractiveOperationsInProgress = false;
                Debug.Log("Setup finished");
            }
        }
    }
}