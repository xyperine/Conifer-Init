using System;

namespace ProjectSetup.Editor
{
    public readonly struct AssetInfo : IEquatable<AssetInfo>
    {
        public string Path { get; }
        public string Name { get; }
        public bool Interactive { get; }


        public AssetInfo(string path, string name, bool interactive)
        {
            Path = path;
            Name = name;
            Interactive = interactive;
        }


        public bool Equals(AssetInfo other)
        {
            return Path == other.Path && Name == other.Name && Interactive == other.Interactive;
        }


        public override bool Equals(object obj)
        {
            return obj is AssetInfo other && Equals(other);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(Path, Name, Interactive);
        }
    }
}