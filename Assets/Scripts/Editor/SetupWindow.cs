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
    public class SetupWindow : EditorWindow
    {
        private const float SPACE_SIZE = 4f;

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

        private List<int> _availablePackagesIndices;
        private readonly List<int> _queuedPackagesIndices = new List<int>(); 
        private int _availablePage = 1;
        private int _queuedPage = 1;
        

        // TODO: Improve/remove the keyboard shortcut
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


        private void OnGUI()
        {
            DrawFolderStructureSettings();

            // Draw packages and plugins settings
            // - Ideally, browse through the packages list and assets list, queue any asset/package for import and
            // option to import it interactively or not.
            // - If the ideal option is problematic, just list essential packages and assets and options to import them
            // interactively or not.

            GUILayout.Space(SPACE_SIZE);

            DrawPackagesSettings();
            
            // Draw scene settings
            // - Add a list scenes and remove/rename the SampleScene

            // Draw project settings
            // - Player settings
            // - Game object naming
            // - Default namespace
            // - Scripting backend
            
            GUILayout.Space(SPACE_SIZE);

            if (GUILayout.Button("Execute Setup", new GUIStyle(GUI.skin.button), GUILayout.Width(128f)))
            {
                string[] folders = _assetsFolderStructureEntry.ToFolderNames();
                Setup.CreateFolders(folders);

                IEnumerable<string> packages =
                    _queuedPackagesIndices.Select(i => _packagesListRequest.Result[i].packageId);
                Setup.ImportPackages(packages);
            }
            
            GUILayout.FlexibleSpace();
        }


        private void DrawFolderStructureSettings()
        {
            GUILayout.Label("Folder Structure", new GUIStyle(EditorStyles.boldLabel));

            if (GUILayout.Button("Reset Structure", new GUIStyle(GUI.skin.button), GUILayout.Width(128f)))
            {
                InitializeRootFSE();
            }
            
            GUILayout.Space(SPACE_SIZE);

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

        
        // TODO: Cleanup
        private void DrawPackagesSettings()
        {
            const int maxEntriesPerPage = 10;
            
            GUILayout.Label("Packages Settings", new GUIStyle(EditorStyles.boldLabel));

            using GUILayout.ScrollViewScope scrollView = new GUILayout.ScrollViewScope(_scrollPosition);
            _scrollPosition = scrollView.scrollPosition;

            if (!SuccessfullyRetrievedPackages(_packagesListRequest))
            {
                return;
            }

            using (new GUILayout.VerticalScope($"Available ({_availablePackagesIndices.Count})", new GUIStyle(GUI.skin.window)))
            {
                int start = (_availablePage - 1) * maxEntriesPerPage;
                int entriesCount = Math.Min(maxEntriesPerPage,
                    _availablePackagesIndices.Count - (_availablePage - 1) * maxEntriesPerPage);
                for (int i = start; i < start + entriesCount; i++)
                {
                    PackageInfo packageInfo = _packagesListRequest.Result[_availablePackagesIndices[i]];
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
                            
                    GUILayout.Label(packageInfo.displayName, new GUIStyle(GUI.skin.label), GUILayout.Height(16f));
                    if (GUILayout.Button("Import", new GUIStyle(GUI.skin.button), GUILayout.Width(64f), GUILayout.Height(16f)))
                    {
                        int index = _availablePackagesIndices[i];
                                
                        _queuedPackagesIndices.Add(index);
                        _availablePackagesIndices.Remove(index);

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
                    Mathf.CeilToInt(_availablePackagesIndices.Count / (float) maxEntriesPerPage);
                GUILayout.Label($"{_availablePage}/{maxPages}", new GUIStyle(GUI.skin.label));
                        
                using (new EditorGUI.DisabledGroupScope(_availablePage >= maxPages))
                {
                    if (GUILayout.Button(">"))
                    {
                        _availablePage++;
                    }
                }
            }

            GUILayout.Space(SPACE_SIZE);
                
            // Queued list
            using (new GUILayout.VerticalScope($"Queued ({_queuedPackagesIndices.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (_queuedPackagesIndices.Count == 0)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("—");
                        GUILayout.FlexibleSpace();
                    }
                }
                else
                {
                    int start = (_queuedPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        _queuedPackagesIndices.Count - (_queuedPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        PackageInfo packageInfo = _packagesListRequest.Result[_queuedPackagesIndices[i]];
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
                                
                        GUILayout.Label(packageInfo.displayName, new GUIStyle(GUI.skin.label), GUILayout.Height(16f));
                                
                        if (GUILayout.Button("Remove", new GUIStyle(GUI.skin.button), GUILayout.Width(64f), GUILayout.Height(16f)))
                        {
                            int index = _queuedPackagesIndices[i];
                                    
                            bool found = false;
                            int originalPrecedingPackageIndex = int.MinValue;
                            int indexOffset = 1;
                            while (!found)
                            {
                                int precedingIndex = Mathf.Max(index - indexOffset, 0);
                                        
                                if (precedingIndex == 0)
                                {
                                    originalPrecedingPackageIndex = -1;
                                    found = true;
                                }
                                        
                                if (_availablePackagesIndices.Contains(precedingIndex))
                                {
                                    originalPrecedingPackageIndex = _availablePackagesIndices.IndexOf(precedingIndex);
                                    found = true;
                                }
                                else
                                {
                                    indexOffset++;
                                }
                            }
                                    
                            _queuedPackagesIndices.Remove(index);
                            _availablePackagesIndices.Insert(originalPrecedingPackageIndex + 1, index);

                            i--;
                            entriesCount--;
                        }
                    }
                        
                    // Pages navigation
                    if (_queuedPackagesIndices.Count > maxEntriesPerPage)
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
                            Mathf.CeilToInt(_queuedPackagesIndices.Count / (float) maxEntriesPerPage);
                        GUILayout.Label($"{_queuedPage}/{maxPages}", new GUIStyle(GUI.skin.label));
                                
                        using (new EditorGUI.DisabledGroupScope(_availablePage >= maxPages))
                        {
                            if (GUILayout.Button(">"))
                            {
                                _queuedPage++;
                            }
                        }
                    }
                }
            }
        }


        private bool SuccessfullyRetrievedPackages(SearchRequest searchRequest)
        {
            if (_successfullyRetrievedPackages)
            {
                return true;
            }
            
            switch (searchRequest.Status)
            {
                case StatusCode.InProgress:
                    GUILayout.Label("Retrieving packages...");
                    _successfullyRetrievedPackages = false;
                    return false;
                case StatusCode.Success:
                    Debug.Log("Successfully retrieved packages.");
                    // Draw the list
                    GUIStyle style1 = new GUIStyle(GUI.skin.label)
                        {normal = new GUIStyleState() {textColor = Color.limeGreen}};
                    GUILayout.Label($"Retrieved packages: {searchRequest.Result.Length}", style1);
                    _availablePackagesIndices = Enumerable.Range(0, searchRequest.Result.Length).ToList();
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
    }
}