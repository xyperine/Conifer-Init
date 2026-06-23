using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Assertions;

namespace ProjectSetup.Editor.Configuration
{
    /// <summary>
    /// Serializes passed settings profile a JSON file. Asserts that passed file names are valid.
    /// </summary>
    internal static class SettingsProfileSerializer
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        };

        public static readonly string ProfilesStoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "xyperine",
            "Project Setup Tool", "Profiles");
        

        public static SettingsProfile ReadFile(string fileName)
        {
            if (!fileName.EndsWith(".json"))
            {
                fileName += ".json";
            }
            
            Assert.IsTrue(IsValidFileName(fileName));
            
            if (!File.Exists(GetFullPath(fileName)))
            {
                return default;
            }

            string json = File.ReadAllText(GetFullPath(fileName));
            return JsonConvert.DeserializeObject<SettingsProfile>(json, SerializerSettings);
        }

        
        public static void SaveFile(SettingsProfile profile, string fileName)
        {
            if (!fileName.EndsWith(".json"))
            {
                fileName += ".json";
            }
            
            Assert.IsTrue(IsValidFileName(fileName));
            
            if (!Directory.Exists(ProfilesStoragePath))
            {
                Directory.CreateDirectory(ProfilesStoragePath);
            }

            string json = JsonConvert.SerializeObject(profile, SerializerSettings);
            File.WriteAllText(GetFullPath(fileName), json);
        }


        public static void DeleteFile(string fileName)
        {
            if (!fileName.EndsWith(".json"))
            {
                fileName += ".json";
            }
            
            Assert.IsTrue(IsValidFileName(fileName));
            
            if (File.Exists(GetFullPath(fileName)))
            {
                File.Delete(GetFullPath(fileName));
            }
        }


        private static bool IsValidFileName(string fileName)
        {
            bool valid = !string.IsNullOrWhiteSpace(fileName);
            valid &= fileName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
            valid &= fileName.EndsWith(".json");
            return valid;
        }


        private static string GetFullPath(string fileName)
        {
            return Path.Combine(ProfilesStoragePath, fileName);
        }
    }
}