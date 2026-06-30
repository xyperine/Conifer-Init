using System;
using UnityEngine;

namespace ConiferInit.Editor.Configuration
{
    [Serializable]
    internal struct MiscSettings : IEquatable<MiscSettings>
    {
        [field: SerializeField] public bool DeleteTutorial { get; set; }
        [field: SerializeField] public bool ConfigureScene { get; set; }
        [field: SerializeField] public string SceneName { get; set; }


        /// <summary>
        /// Creates a default configuration for MiscSettings.
        /// </summary>
        /// <returns>Configured MiscSettings instance.</returns>
        public static MiscSettings Default()
        {
#if CONIFER_INIT_DEV
            return new MiscSettings
            {
                DeleteTutorial = true,
                ConfigureScene = true,
                SceneName = "Main",
            };
#else
            return new MiscSettings
            {
                DeleteTutorial = false,
                ConfigureScene = false,
                SceneName = string.Empty,
            };
#endif
        }


        public bool Equals(MiscSettings other)
        {
            return DeleteTutorial == other.DeleteTutorial && ConfigureScene == other.ConfigureScene &&
                   SceneName == other.SceneName;
        }


        public override bool Equals(object obj)
        {
            return obj is MiscSettings other && Equals(other);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(DeleteTutorial, ConfigureScene, SceneName);
        }
    }
}