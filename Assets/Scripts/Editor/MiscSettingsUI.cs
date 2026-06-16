using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor
{
    public class MiscSettingsUI
    {
        private readonly SetupBusiness _business;


        public MiscSettingsUI(SetupBusiness business)
        {
            _business = business;
        }
        
        
        public void Draw()
        {
            GUILayout.Label("Misc Settings", new GUIStyle(EditorStyles.boldLabel));

            using GUILayout.VerticalScope s = new GUILayout.VerticalScope(new GUIStyle());

            MiscSettings miscSettings = _business.GetMiscSettings();
            
            miscSettings.DeleteTutorial = GUILayout.Toggle(miscSettings.DeleteTutorial, "Delete tutorial");
            miscSettings.ConfigureScene = GUILayout.Toggle(miscSettings.ConfigureScene, "Configure Scene");
            if (miscSettings.ConfigureScene)
            {
                miscSettings.SceneName = EditorGUILayout.TextField("Scene Name", miscSettings.SceneName);
            }

            _business.SetMiscSettings(miscSettings);
        }
    }
}