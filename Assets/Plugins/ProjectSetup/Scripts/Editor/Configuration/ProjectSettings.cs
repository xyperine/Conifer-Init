using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ProjectSetupTool.Editor.Configuration
{
    [Serializable]
    internal struct ProjectSettings : IEquatable<ProjectSettings>
    {
        [field: SerializeField] public string DefaultNamespace { get; set; }
        [field: SerializeField] public EditorSettings.NamingScheme GameObjectNamingScheme { get; set; }
        [field: SerializeField] public string CompanyName { get; set; }
        [field: SerializeField] public string ProductName { get; set; }
        [field: SerializeField] public string Version { get; set; }
        [field: SerializeField] public List<ScriptingBackendEntry> Backends { get; set; }
        
        
        [Serializable]
        public struct ScriptingBackendEntry : IEquatable<ScriptingBackendEntry>
        {
            [field: SerializeField] public NamedBuildTarget Target { get; private set; }
            [field: SerializeField] public ScriptingImplementation Implementation { get; private set; }


            public ScriptingBackendEntry(NamedBuildTarget target, ScriptingImplementation implementation)
            {
                Target = target;
                Implementation = implementation;
            }


            public bool Equals(ScriptingBackendEntry other)
            {
                return Target.Equals(other.Target) && Implementation == other.Implementation;
            }


            public override bool Equals(object obj)
            {
                return obj is ScriptingBackendEntry other && Equals(other);
            }


            public override int GetHashCode()
            {
                return HashCode.Combine(Target, (int) Implementation);
            }
        }
        
        
        public static NamedBuildTarget[] AllTargets { get; } =
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
        };
        
        
        /// <summary>
        /// Creates a default configuration for ProjectSettings.
        /// </summary>
        /// <returns>Configured ProjectSettings instance.</returns>
        public static ProjectSettings Default()
        {
#if SETUP_TOOL_DEV
            return new ProjectSettings("ProjectSetup", EditorSettings.NamingScheme.Underscore, "xyperine",
                "Project Setup", "v0.1.0", ScriptingImplementation.IL2CPP);
#else
            return new ProjectSettings(string.Empty, EditorSettings.NamingScheme.SpaceParenthesis,
                "CompanyName", "ProductName", "0.1.0", ScriptingImplementation.IL2CPP);
#endif
        }
        

        public ProjectSettings(string defaultNamespace, EditorSettings.NamingScheme gameObjectNamingScheme, 
            string companyName, string productName, string version, ScriptingImplementation scriptingBackend)
        {
            DefaultNamespace = defaultNamespace;
            GameObjectNamingScheme = gameObjectNamingScheme;
            CompanyName = companyName;
            ProductName = productName;
            Version = version;

            Backends = AllTargets.Select(t => new ScriptingBackendEntry(t, scriptingBackend)).ToList();
        }


        public ProjectSettings(string defaultNamespace, EditorSettings.NamingScheme gameObjectNamingScheme,
            string companyName, string productName, string version,
            Dictionary<NamedBuildTarget, ScriptingImplementation> backends)
        {
            DefaultNamespace = defaultNamespace;
            GameObjectNamingScheme = gameObjectNamingScheme;
            CompanyName = companyName;
            ProductName = productName;
            Version = version;

            Backends = backends.Select(kvp => new ScriptingBackendEntry(kvp.Key, kvp.Value)).ToList();
        }


        public bool Equals(ProjectSettings other)
        {
            return DefaultNamespace == other.DefaultNamespace &&
                   GameObjectNamingScheme == other.GameObjectNamingScheme && CompanyName == other.CompanyName &&
                   ProductName == other.ProductName && Version == other.Version &&
                   Backends.SequenceEqual(other.Backends);
        }


        public override bool Equals(object obj)
        {
            return obj is ProjectSettings other && Equals(other);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(DefaultNamespace, (int) GameObjectNamingScheme, CompanyName, ProductName, Version,
                Backends);
        }
        
        
        // Just to work with the old design
        public void SetBackend(ScriptingImplementation implementation)
        {
            for (int i = 0; i < Backends.Count; i++)
            {
                ScriptingBackendEntry backend = Backends[i];
                Backends[i] = new ScriptingBackendEntry(backend.Target, implementation);
            }
        }
    }
}