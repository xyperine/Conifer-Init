using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Assertions;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Serializes passed data into a JSON file. Asserts that passed file names are valid.
    /// </summary>
    /// <typeparam name="TData">Type of data to be persisted.</typeparam>
    public static class PersistenceSerializer<TData>
        where TData : new()
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
        

        public static TData ReadFile(string fileName)
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
            return JsonConvert.DeserializeObject<TData>(json, SerializerSettings);
        }

        
        public static void SaveFile(TData data, string fileName)
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

            string json = JsonConvert.SerializeObject(data, SerializerSettings);
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