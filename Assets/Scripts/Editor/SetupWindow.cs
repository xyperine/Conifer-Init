using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Handles drawing logic and user inputs
    /// </summary>
    public class SetupWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        
        // Folder structure settings
        private int _elementIndex;
        
        private bool _isAddingChild;
        private string _newChildName;
        private string _newChildParentFullName;

        private bool _isEditingName;
        private string _editingNameOf;
        private string _newEditedName;

        private FolderStructureEntry _assetsFolderStructureEntry;

        // Packages settings
        private SearchRequest _packagesListRequest;
        private bool _successfullyRetrievedPackages;
        
        [SerializeField] private List<int> queuedPackagesIndices = new List<int>(); 
        private int _availablePage = 1;
        private int _queuedPage = 1;

        private string _packagesSearchString;

        private List<int> AvailablePackages => _successfullyRetrievedPackages && _packagesListRequest?.Result != null
            ? _packagesListRequest.Result.Select(p => Array.IndexOf(_packagesListRequest.Result, p)).Where(i =>
                !queuedPackagesIndices.Contains(i)).ToList()
            : new List<int>();
        
        // Assets settings
        private bool _successfullyRetrievedAssets = false;
        private List<AssetInfo> _assets = new List<AssetInfo>();
        [SerializeField] private List<int> queuedAssetsIndices = new List<int>();
        private int _assetsAvailablePage = 1;
        private int _assetsQueuedPage = 1;

        private string _assetsSearchString;
        
        private List<int> AvailableAssets => _successfullyRetrievedAssets && _assets != null
            ? _assets.Select(a => _assets.IndexOf(a)).Where(i =>
                !queuedAssetsIndices.Contains(i)).ToList()
            : new List<int>();
        
        // Project settings
        private ProjectSettings _projectSettings;
        
        
        [MenuItem("Tools/Setup Window")]
        private static void ShowWindow()
        {
            SetupWindow window = GetWindow<SetupWindow>();
            window.titleContent = new GUIContent("Setup");
            window.Show();
        }


        private void OnEnable()
        {
            InitializeRootFSE();

            _packagesListRequest = Client.SearchAll();

            RetrieveCachedAssets();

            InitializeProjectSettings();
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
                    _assets.Add(new AssetInfo(assetPath, Path.GetFileNameWithoutExtension(assetPath), false));
                }

                _successfullyRetrievedAssets = true;
            }
            else
            {
                throw new DirectoryNotFoundException($"Couldn't find {cachedAssetsPath}");
            }
        }


        private void InitializeRootFSE()
        {
            _assetsFolderStructureEntry = new FolderStructureEntry("Assets", null);
            
            FolderStructureEntry.Create(_assetsFolderStructureEntry, "Animations");
            FolderStructureEntry.Create(_assetsFolderStructureEntry, "Audio");
            FolderStructureEntry.Create(_assetsFolderStructureEntry, "Data/Inputs");
            FolderStructureEntry.Create(_assetsFolderStructureEntry, "Data/URP");
            FolderStructureEntry.Create(_assetsFolderStructureEntry, "Materials");
            FolderStructureEntry.Create(_assetsFolderStructureEntry, "Meshes");
            FolderStructureEntry.Create(_assetsFolderStructureEntry, "Plugins");
            FolderStructureEntry.Create(_assetsFolderStructureEntry, "Prefabs");
            FolderStructureEntry.Create(_assetsFolderStructureEntry, "Shaders");
            FolderStructureEntry.Create(_assetsFolderStructureEntry, "Scripts/Tests/Editor");
            FolderStructureEntry.Create(_assetsFolderStructureEntry, "Scripts/Tests/Runtime");
            FolderStructureEntry.Create(_assetsFolderStructureEntry, "Textures");
        }


        private void InitializeProjectSettings()
        {
            //_projectSettings = new ProjectSettings(string.Empty, EditorSettings.NamingScheme.SpaceParenthesis,
            //    "CompanyName", "ProductName", "0.1.0", ScriptingImplementation.IL2CPP);

            // For simplicity
            _projectSettings = new ProjectSettings("ProjectSetup", EditorSettings.NamingScheme.Underscore, "xyperine",
                "Project Setup", "v0.1.0", ScriptingImplementation.IL2CPP);
        }


        private void OnGUI()
        {
            using GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope(_scrollPosition);
            _scrollPosition = scrollViewScope.scrollPosition;
            
            DrawFolderStructureSettings();

            SetupWindowElements.DrawRegularSpace();

            DrawPackagesSettings();
            
            SetupWindowElements.DrawRegularSpace();
            
            DrawAssetsSettings();

            SetupWindowElements.DrawRegularSpace();
            
            DrawProjectSettings();
            
            SetupWindowElements.DrawRegularSpace();
            
            // Draw scene settings
            // - Add a list scenes and remove/rename the SampleScene
            
            DrawExecuteSetup();
            
            GUILayout.FlexibleSpace();
        }


        private void DrawFolderStructureSettings()
        {
            GUILayout.Label("Folder Structure", new GUIStyle(EditorStyles.boldLabel));

            if (GUILayout.Button("Reset Structure", new GUIStyle(GUI.skin.button), GUILayout.Width(128f)))
            {
                InitializeRootFSE();
            }
            
            SetupWindowElements.DrawRegularSpace();

            _elementIndex = -1;
            GUIStyle foldersSectionStyle = new GUIStyle(GUI.skin.FindStyle("Window"));
            using (new GUILayout.VerticalScope("Hierarchy", foldersSectionStyle))
            {
                DrawHierarchyRecursively(_assetsFolderStructureEntry);
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
            
            foreach (FolderStructureEntry directoryProjection in entry.GetChildren())
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
            
            List<int> assets = AvailableAssets;
            if (!string.IsNullOrWhiteSpace(_assetsSearchString))
            {
                assets = assets.FindAll(index =>
                    _assets[index].Name.Contains(_assetsSearchString,
                        StringComparison.OrdinalIgnoreCase));

                _assetsAvailablePage = 1;
            }
            
            // Available list
            using (new GUILayout.VerticalScope($"Available ({assets.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (assets.Count > 0)
                {
                    int start = (_assetsAvailablePage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        assets.Count - (_assetsAvailablePage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        string assetName = _assets[assets[i]].Name;
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
                            int index = assets[i];
                                    
                            queuedAssetsIndices.Add(index);

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
            using (new GUILayout.VerticalScope($"Queued ({queuedAssetsIndices.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (queuedAssetsIndices.Count > 0)
                {
                    int start = (_assetsQueuedPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        queuedAssetsIndices.Count - (_assetsQueuedPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        AssetInfo assetInfo = _assets[queuedAssetsIndices[i]];
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
                        _assets[queuedAssetsIndices[i]] = new AssetInfo(assetInfo.Path, assetInfo.Name, interactive);

                        if (GUILayout.Button("Remove", new GUIStyle(GUI.skin.button), GUILayout.Width(64f),
                                GUILayout.Height(16f)))
                        {
                            int index = queuedAssetsIndices[i];

                            queuedAssetsIndices.Remove(index);

                            i--;
                            entriesCount--;
                        }
                    }

                    // Pages navigation
                    if (queuedAssetsIndices.Count > maxEntriesPerPage)
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
                            Mathf.CeilToInt(queuedAssetsIndices.Count / (float) maxEntriesPerPage);
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

            _projectSettings.DefaultNamespace =
                EditorGUILayout.TextField("Default Namespace", _projectSettings.DefaultNamespace);
            _projectSettings.GameObjectNamingScheme =
                (EditorSettings.NamingScheme) EditorGUILayout.EnumPopup("Game Object Naming",
                    _projectSettings.GameObjectNamingScheme);
            _projectSettings.CompanyName = EditorGUILayout.TextField("Company Name", _projectSettings.CompanyName);
            _projectSettings.ProductName = EditorGUILayout.TextField("Product Name", _projectSettings.ProductName);
            _projectSettings.Version = EditorGUILayout.TextField("Version", _projectSettings.Version);
            _projectSettings.ScriptingBackend =
                (ScriptingImplementation) EditorGUILayout.EnumPopup("Scripting Backend", _projectSettings.ScriptingBackend);
        }
        

        private void DrawExecuteSetup()
        {
            if (GUILayout.Button("Execute Setup", new GUIStyle(GUI.skin.button), GUILayout.Width(128f)))
            {
                string[] folders = _assetsFolderStructureEntry.ToFolderNames();
                Setup.CreateFolders(folders);

                IEnumerable<string> packages =
                    queuedPackagesIndices.Select(i => _packagesListRequest.Result[i].packageId);
                Setup.ImportPackages(packages);

                IEnumerable<AssetInfo> assets = queuedAssetsIndices.Select(i => _assets[i]);
                Setup.ImportAssets(assets);
                
                Setup.SetProjectSettings(_projectSettings);
            }
        }
    }
}