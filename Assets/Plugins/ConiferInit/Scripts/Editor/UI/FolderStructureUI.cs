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
        
        private readonly Queue<FolderStructureEntry> _entriesToRemove = new Queue<FolderStructureEntry>();

        private GUIStyle _titleStyle;
        private GUIStyle _windowStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _scopeStyle;
        private GUIStyle _hoverableLabelStyle;
        
        private bool _stylesInitialized;
        
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
            if (!_stylesInitialized)
            {
                _titleStyle = new GUIStyle(EditorStyles.boldLabel);
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _windowStyle = new GUIStyle(GUI.skin.window);
                _scopeStyle = new GUIStyle();
                _labelStyle = new GUIStyle(GUI.skin.label);

#if UNITY_6000_1_OR_NEWER
                Color hoveredLabelColor = Color.cornflowerBlue; 
#else
                Color hoveredLabelColor = new Color(0.3921569f, 0.5843138f, 0.9294118f, 1f);
#endif
                _hoverableLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    hover = new GUIStyleState() {textColor = hoveredLabelColor},
                    active = new GUIStyleState() {textColor = hoveredLabelColor},
                };
                
                _stylesInitialized = true;
            }
            
            GUILayout.Label("Folder Structure", _titleStyle);

            
            WindowElements.DrawSmallSpace();

            _elementIndex = -1;
            using (new GUILayout.VerticalScope("Hierarchy", _windowStyle))
            {
                DrawHierarchyRecursively(_configuration.GetAssetsFSE());
            }
            
            if (GUILayout.Button("Reset", _buttonStyle, GUILayout.Width(128f)))
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
            using (EditorGUILayout.HorizontalScope entryScope = new EditorGUILayout.HorizontalScope(_scopeStyle))
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
                    GUILayout.Label(sb.ToString(), _labelStyle, GUILayout.ExpandWidth(false));
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
                        if (GUILayout.Button(indentedName.ToString(), _hoverableLabelStyle) && !_isAddingChild)
                        {
                            _isEditingName = true;
                            _editingNameOf = entry.FullName;
                            _newEditedName = entry.Name;
                        }
                    }
                    else
                    {
                        GUILayout.Label(indentedName.ToString(), _labelStyle);
                    }

                    if (GUILayout.Button("+", _buttonStyle, GUILayout.Width(16f), GUILayout.Height(16f)) && !_isEditingName)
                    {
                        _isAddingChild = true;
                        _newChildParentFullName = entry.FullName;
                        _newChildName = string.Empty;
                    }

                    if (entry.FullName != "Assets")
                    {
                        if (GUILayout.Button("-", _buttonStyle, GUILayout.Width(16f), GUILayout.Height(16f)))
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
                       new EditorGUILayout.HorizontalScope(_scopeStyle))
                {
                    WindowElements.DrawListElementBackground(newFolderScope.rect, _elementIndex);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < depth + 1; i++)
                    {
                        sb.Append("|    ");
                    }

                    GUILayout.Label(sb.ToString(), _labelStyle, GUILayout.ExpandWidth(false));

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