using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Assertions;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Handles drawing logic and user inputs
    /// </summary>
    public class SetupWindow : EditorWindow
    {
        private const string DEFAULT_PROFILE_NAME = "Default_Profile";
        
        private SettingsProfile _settingsProfile;
        private List<SettingsProfile> _profiles;

        private SettingsProfile DefaultProfile => _profiles.Find(p => p.Name == DEFAULT_PROFILE_NAME);
        
        private Vector2 _scrollPosition;
        
        // Folder structure settings
        private int _elementIndex;
        
        private bool _isAddingChild;
        private string _newChildName;
        private string _newChildParentFullName;

        private bool _isEditingName;
        private string _editingNameOf;
        private string _newEditedName;

        // Packages settings
        private SearchRequest _packagesListRequest;
        private bool _successfullyRetrievedPackages;

        private int _availablePage = 1;
        private int _queuedPage = 1;

        private string _packagesSearchString;

        private List<int> AvailablePackages => _successfullyRetrievedPackages && _packagesListRequest?.Result != null
            ? _packagesListRequest.Result.Select(p => Array.IndexOf(_packagesListRequest.Result, p)).Where(i =>
                !ProjectSetupData.instance.QueuedPackagesIndices.Contains(i)).ToList()
            : new List<int>();
        
        // Assets settings
        private bool _successfullyRetrievedAssets = false;
        private List<AssetInfo> _assets = new List<AssetInfo>();
        private int _assetsAvailablePage = 1;
        private int _assetsQueuedPage = 1;

        private string _assetsSearchString;

        private List<string> AvailableAssets => _successfullyRetrievedAssets && _assets != null
            ? _assets.Select(a => a.ID).Where(id =>
                !ProjectSetupData.instance.QueuedAssetIDs.Contains(id)).ToList()
            : new List<string>();
        
        
        [MenuItem("Tools/Setup Window")]
        private static void ShowWindow()
        {
            SetupWindow window = GetWindow<SetupWindow>();
            window.titleContent = new GUIContent("Setup");
            window.Show();
        }


        private void OnEnable()
        {
            LoadSettingsProfiles();

            _packagesListRequest = Client.SearchAll();

            RetrieveCachedAssets();
        }


        private void LoadSettingsProfiles()
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
                    _settingsProfile = p;
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


        private void LoadDefaultProfile()
        {
            if (DefaultProfile == null)
            {
                SettingsProfile defaultProfile = new SettingsProfile()
                {
                    Name = DEFAULT_PROFILE_NAME,
                    AssetsFolderStructureEntry = FolderStructureEntry.Default(),
                    QueuedPackagesIndices = new List<int>(),
                    QueuedAssetIDs = new List<string>(),
                    ProjectSettings = ProjectSettings.Default(),
                    MiscSettings = MiscSettings.Default(),
                };
                    
                PersistenceSerializer<SettingsProfile>.SaveFile(defaultProfile, defaultProfile.Name);
                
                _profiles.Add(defaultProfile);
            }
            
            ApplyProfile(DefaultProfile);
        }


        private void ApplyProfile(SettingsProfile profile)
        {
            // Reset process data
            _isAddingChild = false;
            _newChildName = string.Empty;
            _newChildParentFullName = string.Empty;
            _isEditingName = false;
            _editingNameOf = string.Empty;
            _newEditedName = string.Empty;
            _packagesSearchString = string.Empty;
            _assetsSearchString = string.Empty;
            
            _settingsProfile = profile;
            ProjectSetupData.instance.ActiveSettingsProfileName = _settingsProfile.Name;
            
            ProjectSetupData.instance.AssetsFolderStructureEntry = FolderStructureEntry.DeepCopy(_settingsProfile.AssetsFolderStructureEntry, null);
            ProjectSetupData.instance.QueuedPackagesIndices = new List<int>(_settingsProfile.QueuedPackagesIndices);
            ProjectSetupData.instance.QueuedAssetIDs = new List<string>(_settingsProfile.QueuedAssetIDs);
            ProjectSetupData.instance.ProjectSettings = _settingsProfile.ProjectSettings;
            ProjectSetupData.instance.MiscSettings = _settingsProfile.MiscSettings;
        }
        

        private void RetrieveCachedAssets()
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
                    _assets.Add(new AssetInfo(assetPath, Path.GetFileNameWithoutExtension(assetPath),
                        Path.GetFileNameWithoutExtension(assetPath), false));
                }

                _successfullyRetrievedAssets = true;
            }
            else
            {
                throw new DirectoryNotFoundException($"Couldn't find {cachedAssetsPath}");
            }
        }
        
        
        private void OnGUI()
        {
            using GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope(_scrollPosition);
            _scrollPosition = scrollViewScope.scrollPosition;
            
            DrawSettingsProfileSelection();
            
            SetupWindowElements.DrawRegularSpace();
            
            DrawFolderStructureSettings();

            SetupWindowElements.DrawRegularSpace();

            DrawPackagesSettings();
            
            SetupWindowElements.DrawRegularSpace();
            
            DrawAssetsSettings();

            SetupWindowElements.DrawRegularSpace();
            
            DrawProjectSettings();
            
            SetupWindowElements.DrawRegularSpace();
            
            DrawMiscSettings();
            
            SetupWindowElements.DrawRegularSpace();
            
            DrawExecuteSetup();
            
            GUILayout.FlexibleSpace();
        }


        private void DrawSettingsProfileSelection()
        {
            GUILayout.Label("Profile", new GUIStyle(EditorStyles.boldLabel));

            using (EditorGUI.ChangeCheckScope changeScope = new EditorGUI.ChangeCheckScope())
            {
                string[] profileNames = _profiles.Select(p => p.Name).ToArray();
                int selectedIndex = Array.IndexOf(profileNames, _settingsProfile.Name);
                selectedIndex = EditorGUILayout.Popup("Active Profile", selectedIndex, profileNames,
                    new GUIStyle(EditorStyles.popup),GUILayout.Height(16f));

                if (changeScope.changed)
                {
                    SettingsProfile profile = _profiles.Single(p => p.Name == profileNames[selectedIndex]);
                    ApplyProfile(profile);
                }
            }
            
            using (new GUILayout.HorizontalScope(new GUIStyle()))
            {
                using (EditorGUI.DisabledGroupScope s = new EditorGUI.DisabledGroupScope(_settingsProfile.Name == DefaultProfile.Name))
                {
                    if (GUILayout.Button("Save"))
                    {
                        ConfirmSaveProfileDialog(_settingsProfile);
                    }
                }

                if (GUILayout.Button("Save as..."))
                {
                    ShowSaveProfileFilePanel(path => SaveAsProfile(Path.GetFileNameWithoutExtension(path)));
                }
                
                if (GUILayout.Button("New"))
                {
                    ShowSaveProfileFilePanel(path => CreateNewProfile(Path.GetFileNameWithoutExtension(path)));
                }

                
                using (EditorGUI.DisabledGroupScope s = new EditorGUI.DisabledGroupScope(_settingsProfile.Name == DefaultProfile.Name))
                {
                    if (GUILayout.Button("Delete"))
                    {
                        ConfirmDeleteProfileDialog(_settingsProfile);
                    }
                }

                if (GUILayout.Button("Restore"))
                {
                    ApplyProfile(_settingsProfile);
                }
            }
        }


        private void ConfirmDeleteProfileDialog(SettingsProfile profile)
        {
            Assert.IsFalse(profile.Name == DEFAULT_PROFILE_NAME);
            
            if (EditorDialog.DisplayDecisionDialog("Delete Profile?",
                    $"{profile.Name} profile will be irreversibly deleted. Proceed?",
                    "Yes", "No"))
            {
                DeleteProfile(profile);
            }
        }


        private void DeleteProfile(SettingsProfile profile)
        {
            ApplyProfile(DefaultProfile);
            
            PersistenceSerializer<SettingsProfile>.DeleteFile(profile.Name);
            
            Debug.Log($"Deleted {profile.Name} profile");
            
            LoadSettingsProfiles();
            
            ApplyProfile(DefaultProfile);
        }


        private void ConfirmSaveProfileDialog(SettingsProfile profile)
        {
            Assert.IsFalse(profile.Name == DEFAULT_PROFILE_NAME);
            
            if (_profiles.Exists(p => p.Name == profile.Name))
            {
                if (EditorDialog.DisplayDecisionDialog("Save Profile?",
                        $"This will override the existing {profile.Name} profile. Proceed?",
                        "Yes", "No"))
                {
                    SaveProfile(profile);
                }
            }
            else
            {
                SaveProfile(profile);
            }
        }


        private void SaveProfile(SettingsProfile profile)
        {
            profile.AssetsFolderStructureEntry = ProjectSetupData.instance.AssetsFolderStructureEntry;
            profile.QueuedPackagesIndices = new List<int>(ProjectSetupData.instance.QueuedPackagesIndices);
            profile.QueuedAssetIDs = new List<string>(ProjectSetupData.instance.QueuedAssetIDs);
            profile.ProjectSettings = ProjectSetupData.instance.ProjectSettings;
            profile.MiscSettings = ProjectSetupData.instance.MiscSettings;
            
            PersistenceSerializer<SettingsProfile>.SaveFile(profile, profile.Name);
            
            Debug.Log($"Saved {profile.Name} profile");
            
            LoadSettingsProfiles();
            
            ApplyProfile(profile);
        }


        private void ShowSaveProfileFilePanel(Action<string> onSuccess)
        {
            string newName = "New_Profile";
            if (_profiles.Exists(p => p.Name == newName))
            {
                int i = 1;
                while (_profiles.Any(p => p.Name == newName))
                {
                    newName = "New_Profile" + $"_{i}"; 
                    i++;
                }
            }
            
            string savedPath = EditorUtility.SaveFilePanel("New Profile",
                PersistenceSerializer<SettingsProfile>.ProfilesStoragePath, newName, "json");
            if (savedPath != string.Empty)
            {
                bool insideProfileStorage = Directory.GetParent(savedPath).FullName ==
                             PersistenceSerializer<SettingsProfile>.ProfilesStoragePath;
                bool hasRightExtension = Path.GetExtension(savedPath) == ".json";
                bool isNotDefaultProfile = Path.GetFileNameWithoutExtension(savedPath) != DEFAULT_PROFILE_NAME;
                bool valid = insideProfileStorage && hasRightExtension && isNotDefaultProfile;
                if (valid)
                {
                    onSuccess?.Invoke(savedPath);
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
        }


        private void SaveAsProfile(string newProfileName)
        {
            Debug.Log($"Saving as {newProfileName} profile");
            
            if (newProfileName == string.Empty)
            {
                return;
            }
            
            SettingsProfile profile = new SettingsProfile()
            {
                Name = newProfileName,
            };
            
            ConfirmSaveProfileDialog(profile);
        }


        private void CreateNewProfile(string newProfileName)
        {
            ApplyProfile(DefaultProfile);
            SaveAsProfile(newProfileName);
        }


        private void DrawFolderStructureSettings()
        {
            GUILayout.Label("Folder Structure", new GUIStyle(EditorStyles.boldLabel));

            if (GUILayout.Button("Reset Structure", new GUIStyle(GUI.skin.button), GUILayout.Width(128f)))
            {
                ProjectSetupData.instance.AssetsFolderStructureEntry =
                    FolderStructureEntry.DeepCopy(_settingsProfile.AssetsFolderStructureEntry, null);
            }
            
            SetupWindowElements.DrawRegularSpace();

            _elementIndex = -1;
            GUIStyle foldersSectionStyle = new GUIStyle(GUI.skin.FindStyle("Window"));
            using (new GUILayout.VerticalScope("Hierarchy", foldersSectionStyle))
            {
                DrawHierarchyRecursively(ProjectSetupData.instance.AssetsFolderStructureEntry);
            }
        }


        private void DrawHierarchyRecursively(FolderStructureEntry entry, int depth = 0)
        {
            _elementIndex++;
                
            // Prepend indentation to the name
            StringBuilder indentedName = new StringBuilder();
            for (int i = 0; i < depth; i++)
            {
                indentedName.Append("|    ");
            }

            indentedName.Append(entry.Name);
                
            // Draw the row
            using (EditorGUILayout.HorizontalScope entryScope = new EditorGUILayout.HorizontalScope(new GUIStyle()))
            {
                Color bgColor = _elementIndex % 2 == 0
                    ? new Color(0f, 0f, 0f, 0.03f)
                    : new Color(1f, 1f, 1f, 0.03f);

                Rect rect = new Rect
                {
                    position = entryScope.rect.position,
                    size = entryScope.rect.size,
                };
                EditorGUI.DrawRect(rect, bgColor);

                if (_isEditingName && _editingNameOf == entry.FullName)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < depth; i++)
                    {
                        sb.Append("|    ");
                    }

                    const string textFieldName = "Rename_Text_Field";
                    GUI.SetNextControlName(textFieldName);
                    GUILayout.Label(sb.ToString(), new GUIStyle(GUI.skin.label), GUILayout.ExpandWidth(false));
                    _newEditedName = GUILayout.TextField(_newEditedName, GUILayout.MaxWidth(256f),
                        GUILayout.Height(16f));
                    EditorGUI.FocusTextInControl(textFieldName);

                    if ((GUILayout.Button("Accept", GUILayout.Width(64f), GUILayout.Height(16f)) ||
                         Event.current.keyCode == KeyCode.Return) && IsValidFolderName(_newEditedName))
                    {
                        entry.Rename(_newEditedName);

                        _isEditingName = false;
                        _editingNameOf = string.Empty;
                        _newEditedName = string.Empty;
                    }

                    if (GUILayout.Button("Cancel", GUILayout.Width(64f), GUILayout.Height(16f)) ||
                        Event.current.keyCode == KeyCode.Escape)
                    {
                        _isEditingName = false;
                        _editingNameOf = string.Empty;
                        _newEditedName = string.Empty;
                    }
                }
                else
                {
                    if (entry.FullName != "Assets")
                    {
                        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                        // Really slow to detect the hover for some reason
                        labelStyle.hover = new GUIStyleState() {textColor = Color.cornflowerBlue};
                        if (GUILayout.Button(indentedName.ToString(), labelStyle))
                        {
                            _isEditingName = true;
                            _editingNameOf = entry.FullName;
                            _newEditedName = entry.Name;
                        }
                    }
                    else
                    {
                        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                        GUILayout.Label(indentedName.ToString(), labelStyle);
                    }

                    GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) {fixedWidth = 16, fixedHeight = 16};
                    if (GUILayout.Button("+", buttonStyle))
                    {
                        _isAddingChild = true;
                        _newChildParentFullName = entry.FullName;
                        _newChildName = string.Empty;

                        Debug.Log("Adding child...");
                    }

                    if (GUILayout.Button("-", buttonStyle))
                    {
                        Debug.Log("Removing folder...");
                        entry.Parent.RemoveChild(entry);
                    }
                }
            }
            
            // Draw new folder ui
            if (_isAddingChild && _newChildParentFullName == entry.FullName)
            {
                _elementIndex++;

                using (EditorGUILayout.HorizontalScope newFolderScope =
                       new EditorGUILayout.HorizontalScope(new GUIStyle()))
                {
                    Color bgColor = _elementIndex % 2 == 0
                        ? new Color(0f, 0f, 0f, 0.03f)
                        : new Color(1f, 1f, 1f, 0.03f);

                    Rect rect = new Rect
                    {
                        position = newFolderScope.rect.position,
                        size = newFolderScope.rect.size,
                    };
                    EditorGUI.DrawRect(rect, bgColor);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < depth + 1; i++)
                    {
                        sb.Append("|    ");
                    }

                    GUILayout.Label(sb.ToString(), new GUIStyle(GUI.skin.label), GUILayout.ExpandWidth(false));

                    const string textFieldName = "New_Child_Name_Text_Field";
                    GUI.SetNextControlName(textFieldName);
                    _newChildName = GUILayout.TextField(_newChildName, GUILayout.MaxWidth(256f), GUILayout.Height(16f));
                    EditorGUI.FocusTextInControl(textFieldName);

                    if ((GUILayout.Button("Add", GUILayout.Width(64f), GUILayout.Height(16f)) ||
                         Event.current.keyCode == KeyCode.Return) && IsValidFolderName(_newChildName))
                    {
                        entry.AddChild(new FolderStructureEntry(_newChildName, entry));

                        _newChildName = string.Empty;
                    }

                    if (GUILayout.Button("Cancel", GUILayout.Width(64f), GUILayout.Height(16f)) ||
                        Event.current.keyCode == KeyCode.Escape)
                    {
                        _isAddingChild = false;
                        _newChildParentFullName = string.Empty;
                        _newChildName = string.Empty;
                    }
                }
            }
            
            foreach (FolderStructureEntry directoryProjection in entry.Children)
            {
                DrawHierarchyRecursively(directoryProjection, depth + 1);
            }
        }


        private bool IsValidFolderName(string s)
        {
            bool valid = !string.IsNullOrEmpty(s) && !string.IsNullOrWhiteSpace(s) &&
                         s.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
            return valid;
        }


        private void DrawPackagesSettings()
        {
            const int maxEntriesPerPage = 10;
            
            GUILayout.Label("Packages Settings", new GUIStyle(EditorStyles.boldLabel));

            if (!SuccessfullyRetrievedPackages(_packagesListRequest))
            {
                return;
            }

            // Search feature
            _packagesSearchString =
                GUILayout.TextField(_packagesSearchString, new GUIStyle(EditorStyles.toolbarSearchField),
                    GUILayout.MaxWidth(256f));
            
            List<int> packages = AvailablePackages;
            if (!string.IsNullOrWhiteSpace(_packagesSearchString))
            {
                packages = packages.FindAll(index =>
                    _packagesListRequest.Result[index].displayName.Contains(_packagesSearchString,
                        StringComparison.OrdinalIgnoreCase));

                _availablePage = 1;
            }
            
            // Available list
            List<int> queuedPackagesIndices = ProjectSetupData.instance.QueuedPackagesIndices;
            using (new GUILayout.VerticalScope($"Available ({packages.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (packages.Count > 0)
                {
                    int start = (_availablePage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        packages.Count - (_availablePage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        PackageInfo packageInfo = _packagesListRequest.Result[packages[i]];
                        using EditorGUILayout.HorizontalScope entryScope = new EditorGUILayout.HorizontalScope(new GUIStyle());
                        Color bgColor = i % 2 == 0 
                            ? new Color(0f, 0f, 0f, 0.03f)
                            : new Color(1f, 1f, 1f, 0.03f);

                        Rect rect = new Rect
                        {
                            position = entryScope.rect.position,
                            size = entryScope.rect.size,
                        };
                        EditorGUI.DrawRect(rect, bgColor);
                                
                        GUILayout.Label(packageInfo.displayName, new GUIStyle(GUI.skin.label), GUILayout.Height(16f), GUILayout.MinWidth(128f));
                        if (GUILayout.Button("Import", new GUIStyle(GUI.skin.button), GUILayout.Width(64f), GUILayout.Height(16f)))
                        {
                            int index = packages[i];
                                    
                            queuedPackagesIndices.Add(index);

                            i--;
                        }
                    }

                    // Pages navigation
                    using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(new GUIStyle());
                    GUILayout.FlexibleSpace();
                            
                    using (new EditorGUI.DisabledGroupScope(_availablePage <= 1))
                    {
                        if (GUILayout.Button("<"))
                        {
                            _availablePage--;
                        }
                    }
                            
                    int maxPages =
                        Mathf.CeilToInt(AvailablePackages.Count / (float) maxEntriesPerPage);
                    GUILayout.Label($"{_availablePage}/{maxPages}", new GUIStyle(GUI.skin.label));
                            
                    using (new EditorGUI.DisabledGroupScope(_availablePage >= maxPages))
                    {
                        if (GUILayout.Button(">"))
                        {
                            _availablePage++;
                        }
                    }
                }
                else
                {
                    SetupWindowElements.DrawEmptyListElement();
                }
            }

            SetupWindowElements.DrawRegularSpace();
                
            // Queued list
            using (new GUILayout.VerticalScope($"Queued ({queuedPackagesIndices.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (queuedPackagesIndices.Count > 0)
                {
                    int start = (_queuedPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        queuedPackagesIndices.Count - (_queuedPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        PackageInfo packageInfo = _packagesListRequest.Result[queuedPackagesIndices[i]];
                        using EditorGUILayout.HorizontalScope entryScope =
                            new EditorGUILayout.HorizontalScope(new GUIStyle());
                        Color bgColor = i % 2 == 0
                            ? new Color(0f, 0f, 0f, 0.03f)
                            : new Color(1f, 1f, 1f, 0.03f);

                        Rect rect = new Rect
                        {
                            position = entryScope.rect.position,
                            size = entryScope.rect.size,
                        };
                        EditorGUI.DrawRect(rect, bgColor);

                        GUILayout.Label(packageInfo.displayName, new GUIStyle(GUI.skin.label), GUILayout.Height(16f),
                            GUILayout.MinWidth(128f));

                        if (GUILayout.Button("Remove", new GUIStyle(GUI.skin.button), GUILayout.Width(64f),
                                GUILayout.Height(16f)))
                        {
                            int index = queuedPackagesIndices[i];

                            queuedPackagesIndices.Remove(index);

                            i--;
                            entriesCount--;
                        }
                    }

                    // Pages navigation
                    if (queuedPackagesIndices.Count > maxEntriesPerPage)
                    {
                        using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(new GUIStyle());
                        GUILayout.FlexibleSpace();

                        using (new EditorGUI.DisabledGroupScope(_queuedPage <= 1))
                        {
                            if (GUILayout.Button("<"))
                            {
                                _queuedPage--;
                            }
                        }

                        int maxPages =
                            Mathf.CeilToInt(queuedPackagesIndices.Count / (float) maxEntriesPerPage);
                        GUILayout.Label($"{_queuedPage}/{maxPages}", new GUIStyle(GUI.skin.label));

                        using (new EditorGUI.DisabledGroupScope(_queuedPage >= maxPages))
                        {
                            if (GUILayout.Button(">"))
                            {
                                _queuedPage++;
                            }
                        }
                    }
                }
                else
                {
                    SetupWindowElements.DrawEmptyListElement();
                }
            }
        }


        private bool SuccessfullyRetrievedPackages(SearchRequest searchRequest)
        {
            if (_successfullyRetrievedPackages)
            {
                return searchRequest.Result != null;
            }
            
            switch (searchRequest.Status)
            {
                case StatusCode.InProgress:
                    GUILayout.Label("Retrieving packages...");
                    _successfullyRetrievedPackages = false;
                    return false;
                case StatusCode.Success:
                    Debug.Log("Successfully retrieved packages.");
                    GUIStyle style1 = new GUIStyle(GUI.skin.label)
                        {normal = new GUIStyleState() {textColor = Color.limeGreen}};
                    GUILayout.Label($"Retrieved packages: {searchRequest.Result.Length}", style1);
                    _successfullyRetrievedPackages = true;
                    return true;
                case StatusCode.Failure:
                    GUIStyle style = new GUIStyle(GUI.skin.label)
                        {normal = new GUIStyleState() {textColor = Color.crimson}};
                    GUILayout.Label($"Error while retrieving packages: {searchRequest.Error.message}", style);
                    _successfullyRetrievedPackages = false;
                    return false;
                default:
                    Debug.LogError("Invalid request");
                    _successfullyRetrievedPackages = false;
                    throw new ArgumentOutOfRangeException();
            }
        }


        private void DrawAssetsSettings()
        { 
            const int maxEntriesPerPage = 10;
            
            GUILayout.Label("Assets Settings", new GUIStyle(EditorStyles.boldLabel));

            if (!_successfullyRetrievedAssets)
            {
                RetrieveCachedAssets();
                
                return;
            }
            
            // Search feature
            _assetsSearchString =
                GUILayout.TextField(_assetsSearchString, new GUIStyle(EditorStyles.toolbarSearchField),
                    GUILayout.MaxWidth(256f));
            
            List<string> availableAssetIDs = AvailableAssets;
            if (!string.IsNullOrWhiteSpace(_assetsSearchString))
            {
                availableAssetIDs = availableAssetIDs.FindAll(id =>
                    _assets.Find(a => a.ID == id).Name.Contains(_assetsSearchString,
                        StringComparison.OrdinalIgnoreCase));

                _assetsAvailablePage = 1;
            }
            
            // Available list
            List<string> queuedAssetIDs = ProjectSetupData.instance.QueuedAssetIDs;
            using (new GUILayout.VerticalScope($"Available ({availableAssetIDs.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (availableAssetIDs.Count > 0)
                {
                    int start = (_assetsAvailablePage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        availableAssetIDs.Count - (_assetsAvailablePage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        string assetName = _assets.Find(a => a.ID == availableAssetIDs[i]).Name;
                        using EditorGUILayout.HorizontalScope entryScope = new EditorGUILayout.HorizontalScope(new GUIStyle());
                        Color bgColor = i % 2 == 0 
                            ? new Color(0f, 0f, 0f, 0.03f)
                            : new Color(1f, 1f, 1f, 0.03f);

                        Rect rect = new Rect
                        {
                            position = entryScope.rect.position,
                            size = entryScope.rect.size,
                        };
                        EditorGUI.DrawRect(rect, bgColor);
                                
                        GUILayout.Label(assetName, new GUIStyle(GUI.skin.label), GUILayout.Height(16f), GUILayout.MinWidth(128f));
                        if (GUILayout.Button("Import", new GUIStyle(GUI.skin.button), GUILayout.Width(64f), GUILayout.Height(16f)))
                        {
                            string id = availableAssetIDs[i];
                            queuedAssetIDs.Add(id);

                            i--;
                        }
                    }

                    // Pages navigation
                    using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(new GUIStyle());
                    GUILayout.FlexibleSpace();
                            
                    using (new EditorGUI.DisabledGroupScope(_assetsAvailablePage <= 1))
                    {
                        if (GUILayout.Button("<"))
                        {
                            _assetsAvailablePage--;
                        }
                    }
                            
                    int maxPages =
                        Mathf.CeilToInt(AvailableAssets.Count / (float) maxEntriesPerPage);
                    GUILayout.Label($"{_assetsAvailablePage}/{maxPages}", new GUIStyle(GUI.skin.label));
                            
                    using (new EditorGUI.DisabledGroupScope(_assetsAvailablePage >= maxPages))
                    {
                        if (GUILayout.Button(">"))
                        {
                            _assetsAvailablePage++;
                        }
                    }
                }
                else
                {
                    SetupWindowElements.DrawEmptyListElement();
                }
            }

            SetupWindowElements.DrawRegularSpace();
                
            // Queued list
            using (new GUILayout.VerticalScope($"Queued ({queuedAssetIDs.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (queuedAssetIDs.Count > 0)
                {
                    int start = (_assetsQueuedPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        queuedAssetIDs.Count - (_assetsQueuedPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        AssetInfo assetInfo = _assets.Find(a => a.ID == queuedAssetIDs[i]);
                        using EditorGUILayout.HorizontalScope entryScope =
                            new EditorGUILayout.HorizontalScope(new GUIStyle());
                        Color bgColor = i % 2 == 0
                            ? new Color(0f, 0f, 0f, 0.03f)
                            : new Color(1f, 1f, 1f, 0.03f);

                        Rect rect = new Rect
                        {
                            position = entryScope.rect.position,
                            size = entryScope.rect.size,
                        };
                        EditorGUI.DrawRect(rect, bgColor);

                        GUILayout.Label(assetInfo.Name, new GUIStyle(GUI.skin.label), GUILayout.Height(16f),
                            GUILayout.MinWidth(128f));

                        GUILayout.FlexibleSpace();

                        bool interactive = GUILayout.Toggle(assetInfo.Interactive, "Interactive");
                        _assets[_assets.FindIndex(a => a.ID == queuedAssetIDs[i])] =
                            new AssetInfo(assetInfo.Path, assetInfo.Name, assetInfo.ID, interactive);

                        if (GUILayout.Button("Remove", new GUIStyle(GUI.skin.button), GUILayout.Width(64f),
                                GUILayout.Height(16f)))
                        {
                            string id = queuedAssetIDs[i];
                            queuedAssetIDs.Remove(id);

                            i--;
                            entriesCount--;
                        }
                    }

                    // Pages navigation
                    if (queuedAssetIDs.Count > maxEntriesPerPage)
                    {
                        using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(new GUIStyle());
                        GUILayout.FlexibleSpace();

                        using (new EditorGUI.DisabledGroupScope(_assetsQueuedPage <= 1))
                        {
                            if (GUILayout.Button("<"))
                            {
                                _assetsQueuedPage--;
                            }
                        }

                        int maxPages =
                            Mathf.CeilToInt(queuedAssetIDs.Count / (float) maxEntriesPerPage);
                        GUILayout.Label($"{_assetsQueuedPage}/{maxPages}", new GUIStyle(GUI.skin.label));

                        using (new EditorGUI.DisabledGroupScope(_assetsQueuedPage >= maxPages))
                        {
                            if (GUILayout.Button(">"))
                            {
                                _assetsQueuedPage++;
                            }
                        }
                    }
                }
                else
                {
                    SetupWindowElements.DrawEmptyListElement();
                }
            }
        }
        
        
        private void DrawProjectSettings()
        {
            GUILayout.Label("Project Settings", new GUIStyle(EditorStyles.boldLabel));

            ProjectSettings projectSettings = ProjectSetupData.instance.ProjectSettings;
            projectSettings.DefaultNamespace =
                EditorGUILayout.TextField("Default Namespace", projectSettings.DefaultNamespace);
            projectSettings.GameObjectNamingScheme =
                (EditorSettings.NamingScheme) EditorGUILayout.EnumPopup("Game Object Naming",
                    projectSettings.GameObjectNamingScheme);
            projectSettings.CompanyName = EditorGUILayout.TextField("Company Name", projectSettings.CompanyName);
            projectSettings.ProductName = EditorGUILayout.TextField("Product Name", projectSettings.ProductName);
            projectSettings.Version = EditorGUILayout.TextField("Version", projectSettings.Version);
            projectSettings.ScriptingBackend =
                (ScriptingImplementation) EditorGUILayout.EnumPopup("Scripting Backend", projectSettings.ScriptingBackend);

            ProjectSetupData.instance.ProjectSettings = projectSettings;
        }


        private void DrawMiscSettings()
        {
            GUILayout.Label("Misc Settings", new GUIStyle(EditorStyles.boldLabel));

            using GUILayout.VerticalScope s = new GUILayout.VerticalScope(new GUIStyle());

            MiscSettings miscSettings = ProjectSetupData.instance.MiscSettings;
            miscSettings.DeleteTutorial = GUILayout.Toggle(miscSettings.DeleteTutorial, "Delete tutorial");
            
            miscSettings.ConfigureScene = GUILayout.Toggle(miscSettings.ConfigureScene, "Configure Scene");
            if (miscSettings.ConfigureScene)
            {
                miscSettings.SceneName = EditorGUILayout.TextField("Scene Name", miscSettings.SceneName);
            }

            ProjectSetupData.instance.MiscSettings = miscSettings;
        }
        

        private void DrawExecuteSetup()
        {
            if (GUILayout.Button("Execute Setup", new GUIStyle(GUI.skin.button), GUILayout.Width(128f)))
            {
                string[] folders = ProjectSetupData.instance.AssetsFolderStructureEntry.ToFolderNames();
                Setup.CreateFolders(folders);

                IEnumerable<string> packages =
                    ProjectSetupData.instance.QueuedPackagesIndices.Select(i => _packagesListRequest.Result[i].packageId);
                Setup.ImportPackages(packages);

                IEnumerable<AssetInfo> assets = ProjectSetupData.instance.QueuedAssetIDs.Select(id => _assets.Find(a => a.ID == id));
                Setup.ImportAssets(assets);
                
                Setup.SetProjectSettings(ProjectSetupData.instance.ProjectSettings);
                
                Setup.ExecuteMisc(ProjectSetupData.instance.MiscSettings);
            }
        }
    }
}