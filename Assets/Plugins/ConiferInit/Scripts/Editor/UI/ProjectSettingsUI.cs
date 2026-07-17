using ConiferInit.Editor.Configuration;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor.UI
{
    internal sealed class ProjectSettingsUI
    {
        private readonly SetupConfiguration _configuration;
        private readonly Styles _styles;


        public ProjectSettingsUI(SetupConfiguration configuration, Styles styles)
        {
            _configuration = configuration;
            _styles = styles;
        }
        
        
        public void Draw()
        {
            GUILayout.Label("Project Settings", _styles.SectionTitle);

            using GUILayout.VerticalScope s = new GUILayout.VerticalScope(_styles.Scope);

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