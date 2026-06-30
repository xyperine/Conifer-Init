using ConiferInit.Editor.Configuration;
using UnityEditor;

namespace ConiferInit.Editor.Execution
{
    internal static class ProjectSettingsExecution
    {
        public static void Set(ProjectSettings projectSettings)
        {
            EditorSettings.projectGenerationRootNamespace = projectSettings.DefaultNamespace;
            EditorSettings.gameObjectNamingScheme = projectSettings.GameObjectNamingScheme;
            
            PlayerSettings.companyName = projectSettings.CompanyName;
            PlayerSettings.productName = projectSettings.ProductName;
            PlayerSettings.bundleVersion = projectSettings.Version;
            
            foreach (ProjectSettings.ScriptingBackendEntry backend in projectSettings.Backends)
            {
                PlayerSettings.SetScriptingBackend(backend.Target, backend.Implementation);
            }
        }
    }
}