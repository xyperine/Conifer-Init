using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor.Configuration
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
        internal struct ScriptingBackendEntry : IEquatable<ScriptingBackendEntry>
        {
            [field: SerializeField] public string TargetName { get; private set; }
            [field: SerializeField] public ScriptingImplementation Implementation { get; private set; }


            public ScriptingBackendEntry(string targetName, ScriptingImplementation implementation)
            {
                TargetName = targetName;
                Implementation = implementation;
            }


            public bool Equals(ScriptingBackendEntry other)
            {
                return TargetName.Equals(other.TargetName) && Implementation == other.Implementation;
            }


            public override bool Equals(object obj)
            {
                return obj is ScriptingBackendEntry other && Equals(other);
            }


            public override int GetHashCode()
            {
                return HashCode.Combine(TargetName, (int) Implementation);
            }
        }
        
        
        /// <summary>
        /// Creates a default configuration for ProjectSettings.
        /// </summary>
        /// <returns>Configured ProjectSettings instance.</returns>
        public static ProjectSettings Default()
        {
#if CONIFER_INIT_DEV
            return new ProjectSettings("ConiferInit", EditorSettings.NamingScheme.Underscore, "xyperine",
                "Conifer Init", "0.1.1", ScriptingImplementation.IL2CPP);
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

            Backends = NamedBuildTargetHelpers.AllTargets
                .Select(t => new ScriptingBackendEntry(t.TargetName, scriptingBackend)).ToList();
        }


        public ProjectSettings(string defaultNamespace, EditorSettings.NamingScheme gameObjectNamingScheme,
            string companyName, string productName, string version,
            Dictionary<string, ScriptingImplementation> backends)
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
                Backends[i] = new ScriptingBackendEntry(backend.TargetName, implementation);
            }
        }
    }
}