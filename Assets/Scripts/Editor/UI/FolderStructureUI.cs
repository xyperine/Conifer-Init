using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor.UI
{
    public class FolderStructureUI
    {
        private readonly SetupConfiguration _configuration;
        
        private int _elementIndex;
        
        private bool _isAddingChild;
        private string _newChildName;
        private string _newChildParentFullName;

        private bool _isEditingName;
        private string _editingNameOf;
        private string _newEditedName;


        public FolderStructureUI(SetupConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void ResetTemporaryState()
        {
            _isAddingChild = false;
            _newChildName = string.Empty;
            _newChildParentFullName = string.Empty;
            _isEditingName = false;
            _editingNameOf = string.Empty;
            _newEditedName = string.Empty;
        }
        
        
        public void Draw()
        {
            GUILayout.Label("Folder Structure", new GUIStyle(EditorStyles.boldLabel));

            if (GUILayout.Button("Reset Structure", new GUIStyle(GUI.skin.button), GUILayout.Width(128f)))
            {
                _configuration.ResetFolderStructure();
            }
            
            SetupWindowElements.DrawSmallSpace();

            _elementIndex = -1;
            GUIStyle foldersSectionStyle = new GUIStyle(GUI.skin.FindStyle("Window"));
            using (new GUILayout.VerticalScope("Hierarchy", foldersSectionStyle))
            {
                DrawHierarchyRecursively(_configuration.GetAssetsFSE());
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
                SetupWindowElements.DrawListElementBackground(entryScope.rect, _elementIndex);

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
                        _configuration.RenameFolderStructureEntry(entry, _newEditedName);

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
                        
                        _configuration.RemoveFolderStructureEntry(entry);
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
                        _configuration.AddFolder(_newChildName, entry);
                        
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
            bool valid = !string.IsNullOrWhiteSpace(s) && s.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
            return valid;
        }
    }
}