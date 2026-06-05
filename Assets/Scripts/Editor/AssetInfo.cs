using System;
using UnityEngine;

namespace ProjectSetup.Editor
{
    [Serializable]
    public struct AssetInfo : IEquatable<AssetInfo>
    {
        [field: SerializeField] public string Path { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string ID { get; private set; }
        [field: SerializeField] public bool Interactive { get; private set; }


        public AssetInfo(string path, string name, string id, bool interactive)
        {
            Path = path;
            Name = name;
            ID = id;
            Interactive = interactive;
        }


        public bool Equals(AssetInfo other)
        {
            return Path == other.Path && Name == other.Name && Interactive == other.Interactive && ID == other.ID;
        }


        public override bool Equals(object obj)
        {
            return obj is AssetInfo other && Equals(other);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(Path, Name, ID, Interactive);
        }
    }
}