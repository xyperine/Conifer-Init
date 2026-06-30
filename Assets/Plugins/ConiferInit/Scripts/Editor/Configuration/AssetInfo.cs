using System;

namespace ConiferInit.Editor.Configuration
{
    /// <summary>
    /// Information about an asset in the storage.
    /// </summary>
    internal readonly struct AssetInfo : IEquatable<AssetInfo>
    {
        public string Path { get; }
        public string Name { get; }
        public string ID { get; }


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