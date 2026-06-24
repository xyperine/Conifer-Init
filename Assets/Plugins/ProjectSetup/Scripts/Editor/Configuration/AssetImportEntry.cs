using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ProjectSetupTool.Editor.Configuration
{
    /// <summary>
    /// Information about an asset scheduled for import.
    /// </summary>
    [Serializable]
    internal struct AssetImportEntry : IEquatable<AssetImportEntry>
    {
        [field: SerializeField] public string Path { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string ID { get; private set; }
        [field: SerializeField] public bool Interactive { get; private set; }


        public AssetImportEntry(AssetInfo asset, bool interactive) : this(asset.Path, asset.Name, asset.ID, interactive)
        { }


        [JsonConstructor]
        public AssetImportEntry(string path, string name, string id, bool interactive)
        {
            Path = path;
            Name = name;
            ID = id;
            Interactive = interactive;
        }


        public bool Equals(AssetImportEntry other)
        {
            return Path == other.Path && Name == other.Name && ID == other.ID && Interactive == other.Interactive;
        }


        public override bool Equals(object obj)
        {
            return obj is AssetImportEntry other && Equals(other);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(Path, Name, ID, Interactive);
        }
    }
}