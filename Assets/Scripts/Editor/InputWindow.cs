using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor
{
    public class InputWindow : EditorWindow
    {
        private string _newProfileName;
        private Action<string> _onClosed;

        public event Action<string> WorkFinished; 
        

        public static void Show(Action<string> onClosed)
        {
            var w = GetWindow<InputWindow>(true, "New Profile Name...");
            w._onClosed = onClosed;
            w.ShowModalUtility();
        }


        public string GetName()
        {
            return _newProfileName;
        }
        
        
        private void OnGUI()
        {
            this.minSize = new Vector2(512f, 256f);
            this.maxSize = new Vector2(512f, 256f);
            
            using var s = new GUILayout.VerticalScope(new GUIStyle());
            
            _newProfileName = GUILayout.TextField(_newProfileName, GUILayout.MaxWidth(256f), GUILayout.Height(16f));

            if ((GUILayout.Button("Accept", GUILayout.Width(64f), GUILayout.Height(16f)) ||
                 Event.current.keyCode == KeyCode.Return)) // if done typing name
            {
                _onClosed?.Invoke(_newProfileName);
                Close();
            }
            
            if (GUILayout.Button("Cancel", GUILayout.Width(64f), GUILayout.Height(16f)) ||
                Event.current.keyCode == KeyCode.Escape)
            {
                _newProfileName = string.Empty;
                
                _onClosed?.Invoke(_newProfileName);
                Close();
            }
        }
    }
}