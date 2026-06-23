using System;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor.Configuration
{
    [Serializable]
    internal struct ProjectSettings
    {
        [field: SerializeField] public string DefaultNamespace { get; set; }
        [field: SerializeField] public EditorSettings.NamingScheme GameObjectNamingScheme { get; set; }
        [field: SerializeField] public string CompanyName { get; set; }
        [field: SerializeField] public string ProductName { get; set; }
        [field: SerializeField] public string Version { get; set; }
        [field: SerializeField] public ScriptingImplementation ScriptingBackend { get; set; }


        /// <summary>
        /// Creates a default configuration for ProjectSettings.
        /// </summary>
        /// <returns>Configured ProjectSettings instance.</returns>
        public static ProjectSettings Default()
        {
            // Actual default
            //return new ProjectSettings(string.Empty, EditorSettings.NamingScheme.SpaceParenthesis,
            //    "CompanyName", "ProductName", "0.1.0", ScriptingImplementation.IL2CPP);
            
            // For development
            return new ProjectSettings("ProjectSetup", EditorSettings.NamingScheme.Underscore, "xyperine",
                "Project Setup", "v0.1.0", ScriptingImplementation.IL2CPP);
        }
        
        
        public ProjectSettings(string defaultNamespace, EditorSettings.NamingScheme gameObjectNamingScheme, string companyName, string productName, string version, ScriptingImplementation scriptingBackend)
        {
            DefaultNamespace = defaultNamespace;
            GameObjectNamingScheme = gameObjectNamingScheme;
            CompanyName = companyName;
            ProductName = productName;
            Version = version;
            ScriptingBackend = scriptingBackend;
        }
    }
}