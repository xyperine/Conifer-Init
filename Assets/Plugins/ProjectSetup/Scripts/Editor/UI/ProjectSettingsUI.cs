using ProjectSetupTool.Editor.Configuration;
using UnityEditor;
using UnityEngine;

namespace ProjectSetupTool.Editor.UI
{
    internal sealed class ProjectSettingsUI
    {
        private readonly SetupConfiguration _configuration;


        public ProjectSettingsUI(SetupConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        
        public void Draw()
        {
            GUILayout.Label("Project Settings", new GUIStyle(EditorStyles.boldLabel));

            using GUILayout.VerticalScope s = new GUILayout.VerticalScope(new GUIStyle());

            ProjectSettings projectSettings = _configuration.GetProjectSettings();
            
            projectSettings.DefaultNamespace =
                EditorGUILayout.TextField("Default Namespace", projectSettings.DefaultNamespace);
            projectSettings.GameObjectNamingScheme =
                (EditorSettings.NamingScheme) EditorGUILayout.EnumPopup("Game Object Naming",
                    projectSettings.GameObjectNamingScheme);
            projectSettings.CompanyName = EditorGUILayout.TextField("Company Name", projectSettings.CompanyName);
            projectSettings.ProductName = EditorGUILayout.TextField("Product Name", projectSettings.ProductName);
            projectSettings.Version = EditorGUILayout.TextField("Version", projectSettings.Version);
            ScriptingImplementation scriptingImplementation =
                (ScriptingImplementation) EditorGUILayout.EnumPopup("Scripting Backend",
                    projectSettings.Backends[0].Implementation);
            projectSettings.SetBackend(scriptingImplementation);

            _configuration.SetProjectSettings(projectSettings);
        }
    }
}