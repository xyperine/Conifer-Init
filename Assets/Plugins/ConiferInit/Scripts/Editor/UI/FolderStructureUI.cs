using System.Collections.Generic;
using System.IO;
using System.Text;
using ConiferInit.Editor.Configuration;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor.UI
{
    internal sealed class FolderStructureUI
    {
        private readonly SetupConfiguration _configuration;
        private readonly Styles _styles;
        
        private readonly Queue<FolderStructureEntry> _entriesToRemove = new Queue<FolderStructureEntry>();
        
        private int _elementIndex;
        
        private bool _isAddingChild;
        private string _newChildName;
        private string _newChildParentFullName;

        private bool _isEditingName;
        private string _editingNameOf;
        private string _newEditedName;


        public FolderStructureUI(SetupConfiguration configuration, Styles styles)
        {
            _configuration = configuration;
            _styles = styles;
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
            GUILayout.Label("Folder Structure", _styles.SectionTitle);
            
            WindowElements.DrawSmallSpace();

            _elementIndex = -1;
            using (new GUILayout.VerticalScope("Hierarchy", _styles.List))
            {
                DrawHierarchyRecursively(_configuration.GetAssetsFSE());
            }
            
            if (GUILayout.Button("Reset", _styles.Button, GUILayout.Width(128f)))
            {
                _configuration.ResetFolderStructure();
            }

            while (_entriesToRemove.Count > 0)
            {
                _configuration.RemoveFolderStructureEntry(_entriesToRemove.Dequeue());
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
            using (EditorGUILayout.HorizontalScope entryScope = new EditorGUILayout.HorizontalScope(_styles.Scope))
            {
                WindowElements.DrawListElementBackground(entryScope.rect, _elementIndex);

                if (_isEditingName && _editingNameOf == entry.FullName)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < depth; i++)
                    {
                        sb.Append("|    ");
                    }

                    const string textFieldName = "Rename_Text_Field";
                    GUI.SetNextControlName(textFieldName);
                    GUILayout.Label(sb.ToString(), _styles.Label, GUILayout.ExpandWidth(false));
                    _newEditedName = GUILayout.TextField(_newEditedName, GUILayout.MaxWidth(256f),
                        GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT));
                    EditorGUI.FocusTextInControl(textFieldName);

                    if ((GUILayout.Button("Accept", GUILayout.Width(64f), GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT)) ||
                         Event.current.keyCode == KeyCode.Return) && IsValidFolderName(_newEditedName))
                    {
                        _configuration.RenameFolderStructureEntry(entry, _newEditedName);

                        _isEditingName = false;
                        _editingNameOf = string.Empty;
                        _newEditedName = string.Empty;
                    }

                    if (GUILayout.Button("Cancel", GUILayout.Width(64f), GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT)) ||
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
                        if (GUILayout.Button(indentedName.ToString(), _styles.HoverableLabel) && !_isAddingChild)
                        {
                            _isEditingName = true;
                            _editingNameOf = entry.FullName;
                            _newEditedName = entry.Name;
                        }
                    }
                    else
                    {
                        GUILayout.Label(indentedName.ToString(), _styles.Label);
                    }

                    if (GUILayout.Button("+", _styles.Button, GUILayout.Width(16f), GUILayout.Height(16f)) && !_isEditingName)
                    {
                        _isAddingChild = true;
                        _newChildParentFullName = entry.FullName;
                        _newChildName = string.Empty;
                    }

                    if (entry.FullName != "Assets")
                    {
                        if (GUILayout.Button("-", _styles.Button, GUILayout.Width(16f), GUILayout.Height(16f)))
                        {
                            _entriesToRemove.Enqueue(entry);
                        }
                    }
                }
            }
            
            // Draw new folder ui
            if (_isAddingChild && _newChildParentFullName == entry.FullName)
            {
                _elementIndex++;

                using (EditorGUILayout.HorizontalScope newFolderScope =
                       new EditorGUILayout.HorizontalScope(_styles.Scope))
                {
                    WindowElements.DrawListElementBackground(newFolderScope.rect, _elementIndex);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < depth + 1; i++)
                    {
                        sb.Append("|    ");
                    }

                    GUILayout.Label(sb.ToString(), _styles.Label, GUILayout.ExpandWidth(false));

                    const string textFieldName = "New_Child_Name_Text_Field";
                    GUI.SetNextControlName(textFieldName);
                    _newChildName = GUILayout.TextField(_newChildName, GUILayout.MaxWidth(256f), GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT));
                    EditorGUI.FocusTextInControl(textFieldName);

                    if ((GUILayout.Button("Add", GUILayout.Width(64f), GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT)) ||
                         Event.current.keyCode == KeyCode.Return) && IsValidFolderName(_newChildName))
                    {
                        _configuration.AddFolder(_newChildName, entry);
                        
                        _newChildName = string.Empty;
                    }

                    if (GUILayout.Button("Cancel", GUILayout.Width(64f), GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT)) ||
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