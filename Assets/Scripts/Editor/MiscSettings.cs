using System;
using UnityEngine;

namespace ProjectSetup.Editor
{
    [Serializable]
    internal struct MiscSettings
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
            return new MiscSettings
            {
                DeleteTutorial = true,
                ConfigureScene = true,
                SceneName = "Main",
            };
        }
    }
}