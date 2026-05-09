using System;
using System.Collections.Generic;
using System.IO;
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

        // Folder structure settings
        private bool _isAddingChild;
        private string _newChildName;
        private string _newChildParentFullName;

        private bool _isEditingName;
        private string _editingNameOf;
        private string _newEditedName;

        private FolderStructureEntry _assetsFolderStructureEntry;

        // Packages settings
        private SearchRequest _packagesListRequest;

        private List<PackageInfo> _packagesToImport = new List<PackageInfo>();
        

        // TODO: Improve/remove the keyboard shortcut
        [MenuItem("Tools/Setup")]
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
                var s = _assetsFolderStructureEntry.ToFolderNames();
                Setup.CreateFolders(s);
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
            
            int elementIndex = -1;
            GUIStyle foldersSectionStyle = new GUIStyle(GUI.skin.FindStyle("Window"));
            using (new GUILayout.VerticalScope("Hierarchy", foldersSectionStyle))
            {
                DrawHierarchyRecursively(_assetsFolderStructureEntry);
            }
            
            return;


            void DrawHierarchyRecursively(FolderStructureEntry entry, int depth = 0)
            {
                elementIndex++;
                
                // Prepend indentation to the name
                StringBuilder indentedName = new StringBuilder();
                for (int i = 0; i < depth; i++)
                {
                    indentedName.Append("|    ");
                }

                indentedName.Append(entry.Name);
                
                // Draw the row
                using (EditorGUILayout.HorizontalScope scope = new EditorGUILayout.HorizontalScope(new GUIStyle()))
                {
                    Color bgColor = elementIndex % 2 == 0 
                        ? new Color(0f, 0f, 0f, 0.03f)
                        : new Color(1f, 1f, 1f, 0.03f);

                    Rect rect = new Rect
                    {
                        position = scope.rect.position,
                        size = scope.rect.size,
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
                            
                        if ((GUILayout.Button("Accept", GUILayout.Width(64f), GUILayout.Height(16f)) || Event.current.keyCode == KeyCode.Return) && IsValidFolderName(_newEditedName))
                        {
                            entry.Rename(_newEditedName);

                            _isEditingName = false;
                            _editingNameOf = string.Empty;
                            _newEditedName = string.Empty;
                        }
                        if (GUILayout.Button("Cancel", GUILayout.Width(64f), GUILayout.Height(16f)) || Event.current.keyCode == KeyCode.Escape)
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
                    elementIndex++;
                    
                    using (EditorGUILayout.HorizontalScope scope = new EditorGUILayout.HorizontalScope(new GUIStyle()))
                    {
                        Color bgColor = elementIndex % 2 == 0 
                            ? new Color(0f, 0f, 0f, 0.03f)
                            : new Color(1f, 1f, 1f, 0.03f);

                        Rect rect = new Rect
                        {
                            position = scope.rect.position,
                            size = scope.rect.size,
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
                        
                        if ((GUILayout.Button("Add", GUILayout.Width(64f), GUILayout.Height(16f)) || Event.current.keyCode == KeyCode.Return) && IsValidFolderName(_newChildName))
                        {
                            entry.AddChild(new FolderStructureEntry(_newChildName, entry));

                            _newChildName = string.Empty;
                        }
                        if (GUILayout.Button("Cancel", GUILayout.Width(64f), GUILayout.Height(16f)) || Event.current.keyCode == KeyCode.Escape)
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
            int page = 1;
            
            if (SuccessfullyRetrievedPackages(_packagesListRequest))
            {
                using (new GUILayout.VerticalScope("List", new GUIStyle(GUI.skin.window)))
                {
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        _packagesListRequest.Result.Length - page * maxEntriesPerPage);
                    for (int i = 0; i < entriesCount; i++)
                    {
                        PackageInfo packageInfo = _packagesListRequest.Result[i];
                        using (EditorGUILayout.HorizontalScope s = new EditorGUILayout.HorizontalScope(new GUIStyle()))
                        {
                            Color bgColor = i % 2 == 0 
                                ? new Color(0f, 0f, 0f, 0.03f)
                                : new Color(1f, 1f, 1f, 0.03f);

                            Rect rect = new Rect
                            {
                                position = s.rect.position,
                                size = s.rect.size,
                            };
                            EditorGUI.DrawRect(rect, bgColor);
                            
                            GUILayout.Label(packageInfo.displayName, new GUIStyle(GUI.skin.label), GUILayout.Height(16f));
                            if (GUILayout.Button("Import", new GUIStyle(GUI.skin.button), GUILayout.Width(64f), GUILayout.Height(16f)))
                            {
                                _packagesToImport.Add(packageInfo);
                            }

                            if (GUILayout.Button("Update", new GUIStyle(GUI.skin.button), GUILayout.Width(64f), GUILayout.Height(16f)))
                            {
                                
                            }
                        }
                    }
                }
            }
        }


        private bool SuccessfullyRetrievedPackages(SearchRequest searchRequest)
        {
            switch (searchRequest.Status)
            {
                case StatusCode.InProgress:
                    GUILayout.Label("Retrieving packages...");
                    return false;
                case StatusCode.Success:
                    Debug.Log("Successfully retrieved packages.");
                    // Draw the list
                    GUIStyle style1 = new GUIStyle(GUI.skin.label)
                        {normal = new GUIStyleState() {textColor = Color.limeGreen}};
                    GUILayout.Label($"Retrieved packages: {searchRequest.Result.Length}", style1);
                    return true;
                case StatusCode.Failure:
                    GUIStyle style = new GUIStyle(GUI.skin.label)
                        {normal = new GUIStyleState() {textColor = Color.crimson}};
                    GUILayout.Label($"Error while retrieving packages: {searchRequest.Error.message}", style);
                    return false;
                default:
                    Debug.LogError("Invalid request");
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}