using ConiferInit.Editor.Execution;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor.UI
{
    internal class ExecuteUI
    {
        private readonly SetupExecution _execution;
        
        private GUIStyle _scopeStyle;
        private GUIStyle _executeButtonStyle;
        
        private bool _stylesInitialized;


        public ExecuteUI(SetupExecution execution)
        {
            _execution = execution;
        }


        public void Draw()
        {
            if (!_stylesInitialized)
            {
                _scopeStyle = new GUIStyle();
                _executeButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 16,
                };

                _stylesInitialized = true;
            }
            
            using EditorGUI.DisabledGroupScope ds =
                new EditorGUI.DisabledGroupScope(ExecutionCache.instance.SetupInProgress);
            using GUILayout.HorizontalScope s = new GUILayout.HorizontalScope(_scopeStyle);
            
            if (GUILayout.Button("Execute Setup", _executeButtonStyle, GUILayout.Height(32f)))
            {
                _execution.ExecuteSetup();
            }
        }
    }
}