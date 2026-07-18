using ConiferInit.Editor.Execution;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor.UI
{
    internal sealed class ExecuteUI
    {
        private readonly SetupExecution _execution;
        private readonly Styles _styles;


        public ExecuteUI(SetupExecution execution, Styles styles)
        {
            _execution = execution;
            _styles = styles;
        }


        public void Draw()
        {
            using EditorGUI.DisabledGroupScope ds =
                new EditorGUI.DisabledGroupScope(ExecutionCache.instance.SetupInProgress);
            using GUILayout.HorizontalScope s = new GUILayout.HorizontalScope(_styles.Scope);
            
            if (GUILayout.Button("Execute Setup", _styles.ExecuteButton, GUILayout.Height(32f)))
            {
                _execution.ExecuteSetup();
            }
        }
    }
}