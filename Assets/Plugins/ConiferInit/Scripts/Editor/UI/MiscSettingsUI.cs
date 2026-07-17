using ConiferInit.Editor.Configuration;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor.UI
{
    internal sealed class MiscSettingsUI
    {
        private readonly SetupConfiguration _configuration;
        private readonly Styles _styles;
        

        public MiscSettingsUI(SetupConfiguration configuration, Styles styles)
        {
            _configuration = configuration;
            _styles = styles;
        }
        
        
        public void Draw()
        {
            GUILayout.Label("Misc Settings", _styles.SectionTitle);

            using GUILayout.VerticalScope s = new GUILayout.VerticalScope(_styles.Scope);

            MiscSettings miscSettings = _configuration.GetMiscSettings();
            
            miscSettings.DeleteTutorial = GUILayout.Toggle(miscSettings.DeleteTutorial, "Delete tutorial");
            miscSettings.ConfigureScene = GUILayout.Toggle(miscSettings.ConfigureScene, "Configure Scene");
            if (miscSettings.ConfigureScene)
            {
                miscSettings.SceneName = EditorGUILayout.TextField("Scene Name", miscSettings.SceneName);
            }

            _configuration.SetMiscSettings(miscSettings);
        }
    }
}