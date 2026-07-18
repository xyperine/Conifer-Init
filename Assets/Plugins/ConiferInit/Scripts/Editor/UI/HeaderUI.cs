using System;
using ConiferInit.Editor.Configuration;
using ConiferInit.Editor.Execution;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor.UI
{
    internal sealed class HeaderUI
    {
        private const string LOGO_PATH = "Assets/Plugins/ConiferInit/Textures/Logo.png";
        private const string TITLE_FONT_PATH = "Assets/Plugins/ConiferInit/Fonts/KodeMono-Regular.ttf";

        private readonly SetupConfiguration _configuration;
        private readonly SetupExecution _execution;
        private readonly Styles _styles;
        private readonly Texture2D _logo;
        private readonly Font _font;
        
        public event Action UninstallRequested;
        
        
        public HeaderUI(SetupConfiguration configuration, SetupExecution execution, Styles styles)
        {
            _configuration = configuration;
            _execution = execution;
            _styles = styles;
            _logo = AssetDatabase.LoadAssetAtPath<Texture2D>(LOGO_PATH);
            _font = AssetDatabase.LoadAssetAtPath<Font>(TITLE_FONT_PATH);
        }


        public void Draw()
        {
            DrawBanner();

            WindowElements.DrawSectionSpace();

            DrawToolsOptions();
        }


        private void DrawBanner()
        {
            using (new GUILayout.HorizontalScope(_styles.Scope))
            {
                const float maxSize = 96f;
                
                GUILayout.FlexibleSpace();
                
                GUILayout.Label(_logo, GUILayout.MaxHeight(maxSize), GUILayout.MaxWidth(maxSize));
                
                GUILayout.Space(16f);
                
                GUIStyle windowTitleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 42,
                    alignment = TextAnchor.MiddleLeft,
                    font = _font,
                };
                GUILayout.Label("Conifer Init", windowTitleStyle, GUILayout.Height(maxSize));
                
                GUILayout.FlexibleSpace();
            }
        }
        
        
        private void DrawToolsOptions()
        {
            using GUILayout.HorizontalScope s = new GUILayout.HorizontalScope(_styles.Scope);
            
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset Configuration", _styles.Button, GUILayout.Height(20f), GUILayout.Width(128f)))
            {
                _configuration.ClearCache();
            }
            
            GUILayout.Space(16f);

            if (GUILayout.Button("Reset Execution", _styles.Button, GUILayout.Height(20f), GUILayout.Width(128f)))
            {
                _execution.ClearCache();
            }
            
            GUILayout.Space(16f);

            if (GUILayout.Button("Uninstall", _styles.Button, GUILayout.Height(20f), GUILayout.Width(128f)))
            {
                bool wantToUninstall = Dialog.DisplayDecisionDialog("Uninstall?",
                    "Do you want to remove Conifer Init from your project?", "Yes", "No");
                if (wantToUninstall)
                {
                    UninstallRequested?.Invoke();
                }
            }
            
            GUILayout.FlexibleSpace();
        }
    }
}