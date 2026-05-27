using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace ProjectSetup.Editor
{
    public struct ProjectSettings
    {
        public struct ScriptingBackendEntry
        {
            public NamedBuildTarget BuildTarget { get; set; }
            public ScriptingImplementation ScriptingImplementation { get; set; }


            public ScriptingBackendEntry(NamedBuildTarget buildTarget, ScriptingImplementation scriptingImplementation)
            {
                BuildTarget = buildTarget;
                ScriptingImplementation = scriptingImplementation;
            }
        }
        
        public string DefaultNamespace { get; set; }
        public EditorSettings.NamingScheme GameobjectNamingScheme { get; set; }
        public string CompanyName { get; set; }
        public string ProductName { get; set; }
        public string Version { get; set; }
        public List<ScriptingBackendEntry> ScriptingBackend { get; set; }

        public static readonly NamedBuildTarget[] BuildTargets =
        {
            NamedBuildTarget.Android,
            NamedBuildTarget.EmbeddedLinux,
            NamedBuildTarget.iOS,
            NamedBuildTarget.LinuxHeadlessSimulation,
            NamedBuildTarget.NintendoSwitch,
            NamedBuildTarget.NintendoSwitch2,
            NamedBuildTarget.PS4,
            NamedBuildTarget.PS5,
            NamedBuildTarget.QNX,
            NamedBuildTarget.Server,
            NamedBuildTarget.Standalone,
            NamedBuildTarget.tvOS,
            NamedBuildTarget.VisionOS,
            NamedBuildTarget.WebGL,
            NamedBuildTarget.WindowsStoreApps,
            NamedBuildTarget.XboxOne,
            NamedBuildTarget.Standalone,
        };


        public ProjectSettings(string defaultNamespace, EditorSettings.NamingScheme gameobjectNamingScheme, string companyName, string productName, string version, List<ScriptingBackendEntry> scriptingBackend)
        {
            DefaultNamespace = defaultNamespace;
            GameobjectNamingScheme = gameobjectNamingScheme;
            CompanyName = companyName;
            ProductName = productName;
            Version = version;
            ScriptingBackend = scriptingBackend;
        }
    }
}