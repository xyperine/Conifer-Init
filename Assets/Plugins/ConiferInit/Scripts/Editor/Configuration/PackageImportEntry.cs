using System;
using Newtonsoft.Json;
using UnityEditor.PackageManager;
using UnityEngine;

namespace ConiferInit.Editor.Configuration
{
    /// <summary>
    /// Information about a package scheduled for import.
    /// </summary>
    [Serializable]
    internal struct PackageImportEntry : IEquatable<PackageImportEntry>
    {
        /// <summary>
        /// Used to identify the package within the tool systems.
        /// </summary>
        [field: SerializeField] public string ShortID { get; private set; }
        /// <summary>
        /// Used to actually import the package.
        /// </summary>
        [field: SerializeField] public string FullID { get; private set; }


        public PackageImportEntry(PackageInfo packageInfo) : this(packageInfo.name, packageInfo.packageId)
        { }
        
        
        [JsonConstructor]
        public PackageImportEntry(string shortID, string fullID)
        {
            ShortID = shortID;
            FullID = fullID;
        }


        public bool Equals(PackageImportEntry other)
        {
            return ShortID == other.ShortID && FullID == other.FullID;
        }


        public override bool Equals(object obj)
        {
            return obj is PackageImportEntry other && Equals(other);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(ShortID, FullID);
        }
    }
}