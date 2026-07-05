using ConiferInit.Editor.Configuration;
using UnityEditor;
using UnityEditor.Build;

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
                NamedBuildTarget target = NamedBuildTargetHelpers.FindByName(backend.TargetName);
                PlayerSettings.SetScriptingBackend(target, backend.Implementation);
            }
        }
    }
}