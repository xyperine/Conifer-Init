using System;
using UnityEngine;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Information about an asset scheduled for import.
    /// </summary>
    [Serializable]
    public struct AssetImportEntry
    {
        [field: SerializeField] public string Path { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string ID { get; private set; }
        [field: SerializeField] public bool Interactive { get; private set; }


        public AssetImportEntry(AssetInfo asset, bool interactive) : this(asset.Path, asset.Name, asset.ID, interactive)
        { }


        public AssetImportEntry(string path, string name, string id, bool interactive)
        {
            Path = path;
            Name = name;
            ID = id;
            Interactive = interactive;
        }
    }
}