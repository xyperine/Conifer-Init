using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor
{
    public class ProjectSettingsUI
    {
        private readonly SetupBusiness _business;


        public ProjectSettingsUI(SetupBusiness business)
        {
            _business = business;
        }
        
        
        public void Draw()
        {
            GUILayout.Label("Project Settings", new GUIStyle(EditorStyles.boldLabel));

            using GUILayout.VerticalScope s = new GUILayout.VerticalScope(new GUIStyle());

            ProjectSettings projectSettings = _business.GetProjectSettings();
            
            projectSettings.DefaultNamespace =
                EditorGUILayout.TextField("Default Namespace", projectSettings.DefaultNamespace);
            projectSettings.GameObjectNamingScheme =
                (EditorSettings.NamingScheme) EditorGUILayout.EnumPopup("Game Object Naming",
                    projectSettings.GameObjectNamingScheme);
            projectSettings.CompanyName = EditorGUILayout.TextField("Company Name", projectSettings.CompanyName);
            projectSettings.ProductName = EditorGUILayout.TextField("Product Name", projectSettings.ProductName);
            projectSettings.Version = EditorGUILayout.TextField("Version", projectSettings.Version);
            projectSettings.ScriptingBackend =
                (ScriptingImplementation) EditorGUILayout.EnumPopup("Scripting Backend", projectSettings.ScriptingBackend);

            _business.SetProjectSettings(projectSettings);
        }
    }
}