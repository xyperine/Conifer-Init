using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Assertions;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Handles drawing logic and user inputs.
    /// </summary>
    public class SetupWindow : EditorWindow
    {
        private readonly SetupBusiness _business = new SetupBusiness();
        
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
        private int _availablePackagesPage = 1;
        private int _queuedPackagesPage = 1;

        private string _packagesSearchString;
        
        // Assets settings
        private int _availableAssetsPage = 1;
        private int _queuedAssetsPage = 1;

        private string _assetsSearchString;
        
        
        [MenuItem("Tools/Setup Window")]
        private static void ShowWindow()
        {
            SetupWindow window = GetWindow<SetupWindow>();
            window.titleContent = new GUIContent("Setup");
            window.Show();
        }


        private void OnEnable()
        {
            _business.Initialize();
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
                string[] profileNames = _business.Profiles.Select(p => p.Name).ToArray();
                int selectedIndex = Array.IndexOf(profileNames, _business.ActiveProfile.Name);
                selectedIndex = EditorGUILayout.Popup("Active Profile", selectedIndex, profileNames,
                    new GUIStyle(EditorStyles.popup),GUILayout.Height(16f));

                if (changeScope.changed)
                {
                    SettingsProfile profile = _business.Profiles.Single(p => p.Name == profileNames[selectedIndex]);
                    ApplyProfile(profile);
                }
            }
            
            using (new GUILayout.HorizontalScope(new GUIStyle()))
            {
                using (EditorGUI.DisabledGroupScope s = new EditorGUI.DisabledGroupScope(_business.ActiveProfile.Name == _business.DefaultProfile.Name))
                {
                    if (GUILayout.Button("Save"))
                    {
                        ConfirmSaveProfileDialog(_business.ActiveProfile);
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
                
                using (EditorGUI.DisabledGroupScope s = new EditorGUI.DisabledGroupScope(_business.ActiveProfile.Name == _business.DefaultProfile.Name))
                {
                    if (GUILayout.Button("Delete"))
                    {
                        ConfirmDeleteProfileDialog(_business.ActiveProfile);
                    }
                }

                if (GUILayout.Button("Restore"))
                {
                    ApplyProfile(_business.ActiveProfile);
                }
            }
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
            
            _business.ApplyProfile(profile);
        }


        private void ConfirmDeleteProfileDialog(SettingsProfile profile)
        {
            Assert.IsFalse(profile.Name == SetupBusiness.DEFAULT_PROFILE_NAME);
            
            if (EditorDialog.DisplayDecisionDialog("Delete Profile?",
                    $"{profile.Name} profile will be irreversibly deleted. Proceed?",
                    "Yes", "No"))
            {
                _business.DeleteProfile(profile);
            }
        }


        private void ConfirmSaveProfileDialog(SettingsProfile profile)
        {
            Assert.IsFalse(profile.Name == SetupBusiness.DEFAULT_PROFILE_NAME);
            
            if (_business.Profiles.Exists(p => p.Name == profile.Name))
            {
                if (EditorDialog.DisplayDecisionDialog("Save Profile?",
                        $"This will override the existing {profile.Name} profile. Proceed?",
                        "Yes", "No"))
                {
                    _business.SaveProfile(profile);
                }
            }
            else
            {
                _business.SaveProfile(profile);
            }
        }


        private void ShowSaveProfileFilePanel(Action<string> onSuccess)
        {
            string newName = _business.ConstructNewProfileName();
            
            string savedPath = EditorUtility.SaveFilePanel("New Profile",
                PersistenceSerializer<SettingsProfile>.ProfilesStoragePath, newName, "json");
            if (savedPath != string.Empty)
            {
                _business.TrySaveProfileAt(savedPath, onSuccess);
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
            ApplyProfile(_business.DefaultProfile);
            SaveAsProfile(newProfileName);
        }


        private void DrawFolderStructureSettings()
        {
            GUILayout.Label("Folder Structure", new GUIStyle(EditorStyles.boldLabel));

            if (GUILayout.Button("Reset Structure", new GUIStyle(GUI.skin.button), GUILayout.Width(128f)))
            {
                _business.ResetFolderStructure();
            }
            
            SetupWindowElements.DrawRegularSpace();

            _elementIndex = -1;
            GUIStyle foldersSectionStyle = new GUIStyle(GUI.skin.FindStyle("Window"));
            using (new GUILayout.VerticalScope("Hierarchy", foldersSectionStyle))
            {
                DrawHierarchyRecursively(_business.GetAssetsFSE());
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
                        _business.RenameFolderStructureEntry(entry, _newEditedName);

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
                        if (GUILayout.Button(indentedName.ToString(), labelStyle) && !_isAddingChild)
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
                    if (GUILayout.Button("+", buttonStyle) && !_isEditingName)
                    {
                        _isAddingChild = true;
                        _newChildParentFullName = entry.FullName;
                        _newChildName = string.Empty;

                        Debug.Log("Adding child...");
                    }

                    if (GUILayout.Button("-", buttonStyle))
                    {
                        Debug.Log("Removing folder...");
                        
                        _business.RemoveFolderStructureEntry(entry);
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
                        _business.AddFolder(_newChildName, entry);
                        
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
            
            foreach (FolderStructureEntry childEntry in entry.Children)
            {
                DrawHierarchyRecursively(childEntry, depth + 1);
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

            if (!SuccessfullyRetrievedPackages())
            {
                return;
            }

            // Search feature
            _packagesSearchString =
                GUILayout.TextField(_packagesSearchString, new GUIStyle(EditorStyles.toolbarSearchField),
                    GUILayout.MaxWidth(256f));
            
            List<string> availablePackageIDs = _business.AvailablePackages;
            if (!string.IsNullOrWhiteSpace(_packagesSearchString))
            {
                availablePackageIDs = _business.FindPackages(_packagesSearchString);

                _availablePackagesPage = 1;
            }
            
            // Available list
            List<string> queuedPackageIDs = _business.GetQueuedPackageIDs();
            using (new GUILayout.VerticalScope($"Available ({availablePackageIDs.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (availablePackageIDs.Count > 0)
                {
                    int start = (_availablePackagesPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        availablePackageIDs.Count - (_availablePackagesPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        PackageInfo packageInfo = _business.GetPackageByID(availablePackageIDs[i]);
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
                            _business.QueuePackage(availablePackageIDs[i]);

                            i--;
                        }
                    }

                    // Pages navigation
                    using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(new GUIStyle());
                    GUILayout.FlexibleSpace();
                            
                    using (new EditorGUI.DisabledGroupScope(_availablePackagesPage <= 1))
                    {
                        if (GUILayout.Button("<"))
                        {
                            _availablePackagesPage--;
                        }
                    }
                            
                    int maxPages =
                        Mathf.CeilToInt(availablePackageIDs.Count / (float) maxEntriesPerPage);
                    GUILayout.Label($"{_availablePackagesPage}/{maxPages}", new GUIStyle(GUI.skin.label));
                            
                    using (new EditorGUI.DisabledGroupScope(_availablePackagesPage >= maxPages))
                    {
                        if (GUILayout.Button(">"))
                        {
                            _availablePackagesPage++;
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
            using (new GUILayout.VerticalScope($"Queued ({queuedPackageIDs.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (queuedPackageIDs.Count > 0)
                {
                    int start = (_queuedPackagesPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        queuedPackageIDs.Count - (_queuedPackagesPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        PackageInfo packageInfo = _business.GetPackageByID(queuedPackageIDs[i]);
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
                            _business.DequeuePackage(queuedPackageIDs[i]);

                            i--;
                            entriesCount--;
                        }
                    }

                    // Pages navigation
                    if (queuedPackageIDs.Count > maxEntriesPerPage)
                    {
                        using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(new GUIStyle());
                        GUILayout.FlexibleSpace();

                        using (new EditorGUI.DisabledGroupScope(_queuedPackagesPage <= 1))
                        {
                            if (GUILayout.Button("<"))
                            {
                                _queuedPackagesPage--;
                            }
                        }

                        int maxPages =
                            Mathf.CeilToInt(queuedPackageIDs.Count / (float) maxEntriesPerPage);
                        GUILayout.Label($"{_queuedPackagesPage}/{maxPages}", new GUIStyle(GUI.skin.label));

                        using (new EditorGUI.DisabledGroupScope(_queuedPackagesPage >= maxPages))
                        {
                            if (GUILayout.Button(">"))
                            {
                                _queuedPackagesPage++;
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


        // This routine is poorly designed so I don't know what to do with it 
        private bool SuccessfullyRetrievedPackages()
        {
            switch (_business.PackagesListRequest.Status)
            {
                case StatusCode.InProgress:
                    GUILayout.Label("Retrieving packages...");
                    break;
                case StatusCode.Success:
                    //Debug.Log("Successfully retrieved packages.");
                    GUIStyle style1 = new GUIStyle(GUI.skin.label)
                        {normal = new GUIStyleState() {textColor = Color.limeGreen}};
                    GUILayout.Label($"Retrieved packages: {_business.PackagesListRequest.Result.Length}", style1);
                    break;
                case StatusCode.Failure:
                    //Debug.Log("Failed to retrieve packages.");
                    GUIStyle style = new GUIStyle(GUI.skin.label)
                        {normal = new GUIStyleState() {textColor = Color.crimson}};
                    GUILayout.Label($"Error while retrieving packages: {_business.PackagesListRequest.Error.message}", style);
                    break;
                default:
                    Debug.LogError("Invalid request");
                    throw new ArgumentOutOfRangeException();
            }
            
            return _business.SuccessfullyRetrievedPackages();
        }


        private void DrawAssetsSettings()
        { 
            const int maxEntriesPerPage = 10;
            
            GUILayout.Label("Assets Settings", new GUIStyle(EditorStyles.boldLabel));

            if (!_business.SuccessfullyRetrievedAssets)
            {
                _business.RetrieveCachedAssets();
                
                return;
            }
            
            // Search feature
            _assetsSearchString =
                GUILayout.TextField(_assetsSearchString, new GUIStyle(EditorStyles.toolbarSearchField),
                    GUILayout.MaxWidth(256f));
            
            List<string> availableAssetIDs = _business.AvailableAssets;
            if (!string.IsNullOrWhiteSpace(_assetsSearchString))
            {
                availableAssetIDs = _business.FindAssets(_assetsSearchString);

                _availableAssetsPage = 1;
            }
            
            // Available list
            List<AssetImportEntry> queuedAssets = _business.GetQueuedAssets();
            using (new GUILayout.VerticalScope($"Available ({availableAssetIDs.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (availableAssetIDs.Count > 0)
                {
                    int start = (_availableAssetsPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        availableAssetIDs.Count - (_availableAssetsPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        string assetName = _business.FindAssetByID(availableAssetIDs[i]).Name;
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
                            _business.QueueAsset(availableAssetIDs[i]);

                            i--;
                        }
                    }

                    // Pages navigation
                    using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(new GUIStyle());
                    GUILayout.FlexibleSpace();
                            
                    using (new EditorGUI.DisabledGroupScope(_availableAssetsPage <= 1))
                    {
                        if (GUILayout.Button("<"))
                        {
                            _availableAssetsPage--;
                        }
                    }
                            
                    int maxPages =
                        Mathf.CeilToInt(availableAssetIDs.Count / (float) maxEntriesPerPage);
                    GUILayout.Label($"{_availableAssetsPage}/{maxPages}", new GUIStyle(GUI.skin.label));
                            
                    using (new EditorGUI.DisabledGroupScope(_availableAssetsPage >= maxPages))
                    {
                        if (GUILayout.Button(">"))
                        {
                            _availableAssetsPage++;
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
            using (new GUILayout.VerticalScope($"Queued ({queuedAssets.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (queuedAssets.Count > 0)
                {
                    int start = (_queuedAssetsPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        queuedAssets.Count - (_queuedAssetsPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        AssetImportEntry asset = queuedAssets[i];
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

                        GUILayout.Label(asset.Name, new GUIStyle(GUI.skin.label), GUILayout.Height(16f),
                            GUILayout.MinWidth(128f));

                        GUILayout.FlexibleSpace();

                        bool interactive = GUILayout.Toggle(asset.Interactive, "Interactive");
                        _business.SetInteractiveImportForAsset(asset.ID, interactive);

                        if (GUILayout.Button("Remove", new GUIStyle(GUI.skin.button), GUILayout.Width(64f),
                                GUILayout.Height(16f)))
                        {
                            _business.DequeueAsset(asset.ID);

                            i--;
                            entriesCount--;
                        }
                    }

                    // Pages navigation
                    if (queuedAssets.Count > maxEntriesPerPage)
                    {
                        using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(new GUIStyle());
                        GUILayout.FlexibleSpace();

                        using (new EditorGUI.DisabledGroupScope(_queuedAssetsPage <= 1))
                        {
                            if (GUILayout.Button("<"))
                            {
                                _queuedAssetsPage--;
                            }
                        }

                        int maxPages =
                            Mathf.CeilToInt(queuedAssets.Count / (float) maxEntriesPerPage);
                        GUILayout.Label($"{_queuedAssetsPage}/{maxPages}", new GUIStyle(GUI.skin.label));

                        using (new EditorGUI.DisabledGroupScope(_queuedAssetsPage >= maxPages))
                        {
                            if (GUILayout.Button(">"))
                            {
                                _queuedAssetsPage++;
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

            using GUILayout.VerticalScope s = new GUILayout.VerticalScope(new GUIStyle());

            ProjectSettings projectSettings = _business.GetProjectSettings();
            
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

            _business.SetProjectSettings(projectSettings);
        }


        private void DrawMiscSettings()
        {
            GUILayout.Label("Misc Settings", new GUIStyle(EditorStyles.boldLabel));

            using GUILayout.VerticalScope s = new GUILayout.VerticalScope(new GUIStyle());

            MiscSettings miscSettings = _business.GetMiscSettings();
            
            miscSettings.DeleteTutorial = GUILayout.Toggle(miscSettings.DeleteTutorial, "Delete tutorial");
            miscSettings.ConfigureScene = GUILayout.Toggle(miscSettings.ConfigureScene, "Configure Scene");
            if (miscSettings.ConfigureScene)
            {
                miscSettings.SceneName = EditorGUILayout.TextField("Scene Name", miscSettings.SceneName);
            }

            _business.SetMiscSettings(miscSettings);
        }


        private void DrawExecuteSetup()
        {
            if (GUILayout.Button("Execute Setup", new GUIStyle(GUI.skin.button), GUILayout.Width(128f)))
            {
                _business.ExecuteSetup();
            }
        }
        
        
        private void Update()
        {
            _business.Update();
        }
    }
}