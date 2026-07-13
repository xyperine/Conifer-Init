using System;
using ConiferInit.Editor.Configuration;
using ConiferInit.Editor.Execution;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor.UI
{
    internal class HeaderUI
    {
        private const string LOGO_PATH = "Assets/Plugins/ConiferInit/Textures/Logo.png";
        private const string TITLE_FONT_PATH = "Assets/Plugins/ConiferInit/Fonts/KodeMono-Regular.ttf";

        private readonly SetupConfiguration _configuration;
        private readonly SetupExecution _execution;
        private readonly Texture2D _logo;
        private readonly Font _font;
        
        private GUIStyle _scopeStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _buttonStyle;

        private bool _stylesInitialized;

        public event Action UninstallRequested;
        
        
        public HeaderUI(SetupConfiguration configuration, SetupExecution execution)
        {
            _configuration = configuration;
            _execution = execution;
            _logo = AssetDatabase.LoadAssetAtPath<Texture2D>(LOGO_PATH);
            _font = AssetDatabase.LoadAssetAtPath<Font>(TITLE_FONT_PATH);
        }


        public void Draw()
        {
            if (!_stylesInitialized)
            {
                _scopeStyle = new GUIStyle();
                _titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 42,
                    alignment = TextAnchor.MiddleLeft,
                    font = _font,
                };
                _buttonStyle = new GUIStyle(GUI.skin.button);

                _stylesInitialized = true;
            }
            
            using (new GUILayout.HorizontalScope(_scopeStyle))
            {
                const float maxSize = 96f;
                GUILayout.FlexibleSpace();
                GUILayout.Label(_logo, GUILayout.MaxHeight(maxSize), GUILayout.MaxWidth(maxSize));
                GUILayout.Space(16f);
                GUILayout.Label("Conifer Init", _titleStyle, GUILayout.Height(maxSize));
                GUILayout.FlexibleSpace();
            }
            
            WindowElements.DrawSectionSpace();

            DrawToolsOptions();
        }
        
        
        private void DrawToolsOptions()
        {
            using GUILayout.HorizontalScope s = new GUILayout.HorizontalScope(_scopeStyle);
            
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset Configuration", _buttonStyle, GUILayout.Height(20f), GUILayout.Width(128f)))
            {
                _configuration.ClearCache();
            }
            
            GUILayout.Space(16f);

            if (GUILayout.Button("Reset Execution", _buttonStyle, GUILayout.Height(20f), GUILayout.Width(128f)))
            {
                _execution.ClearCache();
            }
            
            GUILayout.Space(16f);

            if (GUILayout.Button("Uninstall", _buttonStyle, GUILayout.Height(20f), GUILayout.Width(128f)))
            {
#if UNITY_6000_3_OR_NEWER
                bool wantToUninstall = EditorDialog.DisplayDecisionDialog("Uninstall?",
                    "Do you want to remove Conifer Init from your project?", "Yes", "No");
#else
                bool wantToUninstall = EditorUtility.DisplayDialog("Uninstall?",
                    "Do you want to remove Conifer Init from your project?", "Yes", "No");
#endif
                if (wantToUninstall)
                {
                    UninstallRequested?.Invoke();
                }
            }
            
            GUILayout.FlexibleSpace();
        }
    }
}