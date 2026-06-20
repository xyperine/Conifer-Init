using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor.UI
{
    internal sealed class MiscSettingsUI
    {
        private readonly SetupConfiguration _configuration;


        public MiscSettingsUI(SetupConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        
        public void Draw()
        {
            GUILayout.Label("Misc Settings", new GUIStyle(EditorStyles.boldLabel));

            using GUILayout.VerticalScope s = new GUILayout.VerticalScope(new GUIStyle());

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