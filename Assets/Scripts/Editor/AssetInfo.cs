using System;
using UnityEngine;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Information about an asset in the storage.
    /// </summary>
    [Serializable]
    public struct AssetInfo : IEquatable<AssetInfo>
    {
        [field: SerializeField] public string Path { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string ID { get; private set; }


        public AssetInfo(string path, string name, string id)
        {
            Path = path;
            Name = name;
            ID = id;
        }


        public bool Equals(AssetInfo other)
        {
            return Path == other.Path && Name == other.Name && ID == other.ID;
        }


        public override bool Equals(object obj)
        {
            return obj is AssetInfo other && Equals(other);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(Path, Name, ID);
        }
    }
}