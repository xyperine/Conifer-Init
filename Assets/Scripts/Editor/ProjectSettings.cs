using UnityEditor;

namespace ProjectSetup.Editor
{
    public struct ProjectSettings
    {
        public string DefaultNamespace { get; set; }
        public EditorSettings.NamingScheme GameObjectNamingScheme { get; set; }
        public string CompanyName { get; set; }
        public string ProductName { get; set; }
        public string Version { get; set; }
        public ScriptingImplementation ScriptingBackend { get; set; }

        
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