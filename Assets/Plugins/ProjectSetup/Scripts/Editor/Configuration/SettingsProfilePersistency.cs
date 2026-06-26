using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Build;
using UnityEngine.Assertions;

namespace ProjectSetupTool.Editor.Configuration
{
    /// <summary>
    /// Serializes passed settings profile a JSON file. Asserts that passed file names are valid.
    /// </summary>
    internal static class SettingsProfilePersistency
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Converters =
            {
                new NamedBuildTargetConverter(),
            },
        };

        public static readonly string StoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "xyperine",
            "Project Setup Tool", "Profiles");
        

        public static SettingsProfile Restore(string fileName)
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

        
        public static void Save(SettingsProfile profile)
        {
            string fileName = profile.Name + ".json";
            
            Assert.IsTrue(IsValidFileName(fileName));
            
            if (!Directory.Exists(StoragePath))
            {
                Directory.CreateDirectory(StoragePath);
            }

            string json = JsonConvert.SerializeObject(profile, SerializerSettings);
            File.WriteAllText(GetFullPath(fileName), json);
        }


        public static void Delete(SettingsProfile profile)
        {
            Delete(profile.Name);
        }
        
        
        public static void Delete(string fileName)
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
            return Path.Combine(StoragePath, fileName);
        }
    }
    
    
    internal sealed class NamedBuildTargetConverter : JsonConverter<NamedBuildTarget>
    {
        private readonly Dictionary<string, NamedBuildTarget> _namedBuildTargets = new Dictionary<string, NamedBuildTarget>
        {
            {NamedBuildTarget.Android.TargetName, NamedBuildTarget.Android},
            {NamedBuildTarget.EmbeddedLinux.TargetName, NamedBuildTarget.EmbeddedLinux},
            {NamedBuildTarget.iOS.TargetName, NamedBuildTarget.iOS},
            {NamedBuildTarget.LinuxHeadlessSimulation.TargetName, NamedBuildTarget.LinuxHeadlessSimulation},
            {NamedBuildTarget.NintendoSwitch.TargetName, NamedBuildTarget.NintendoSwitch},
            {NamedBuildTarget.NintendoSwitch2.TargetName, NamedBuildTarget.NintendoSwitch2},
            {NamedBuildTarget.PS4.TargetName, NamedBuildTarget.PS4},
            {NamedBuildTarget.PS5.TargetName, NamedBuildTarget.PS5},
            {NamedBuildTarget.QNX.TargetName, NamedBuildTarget.QNX},
            {NamedBuildTarget.Server.TargetName, NamedBuildTarget.Server},
            {NamedBuildTarget.Standalone.TargetName, NamedBuildTarget.Standalone},
            {NamedBuildTarget.tvOS.TargetName, NamedBuildTarget.tvOS},
            {NamedBuildTarget.VisionOS.TargetName, NamedBuildTarget.VisionOS},
            {NamedBuildTarget.WebGL.TargetName, NamedBuildTarget.WebGL},
            {NamedBuildTarget.WindowsStoreApps.TargetName, NamedBuildTarget.WindowsStoreApps},
            {NamedBuildTarget.XboxOne.TargetName, NamedBuildTarget.XboxOne},
        };
        
        
        public override void WriteJson(JsonWriter writer, NamedBuildTarget value, JsonSerializer serializer)
        {
            JObject obj = new JObject()
            {
                new JProperty("Name", value.TargetName),
            };
            
            obj.WriteTo(writer);
        }


        public override NamedBuildTarget ReadJson(JsonReader reader, Type objectType, NamedBuildTarget existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            string name = obj.Value<string>("Name");

            return _namedBuildTargets[name];
        }
    }
}