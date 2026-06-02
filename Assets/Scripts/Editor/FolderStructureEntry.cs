using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Describes a folder in the Assets directory. Paths are relative to the Assets directory. Implements a tree structure.
    /// </summary>
    [Serializable]
    public class FolderStructureEntry : IEquatable<FolderStructureEntry>
    {
        [JsonProperty] private List<FolderStructureEntry> _children;
            
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public FolderStructureEntry Parent { get; private set; }
        
        public string FullName =>
            Path.Combine(Parent == null || Parent.FullName == "Assets" 
                ? string.Empty 
                : Parent.FullName
                , Name);


        public static FolderStructureEntry Default()
        {
            FolderStructureEntry assetsFolderStructureEntry = new FolderStructureEntry("Assets", (FolderStructureEntry) null);
            
            Create(assetsFolderStructureEntry, "Animations");
            Create(assetsFolderStructureEntry, "Audio");
            Create(assetsFolderStructureEntry, "Data/Inputs");
            Create(assetsFolderStructureEntry, "Data/URP");
            Create(assetsFolderStructureEntry, "Materials");
            Create(assetsFolderStructureEntry, "Meshes");
            Create(assetsFolderStructureEntry, "Plugins");
            Create(assetsFolderStructureEntry, "Prefabs");
            Create(assetsFolderStructureEntry, "Shaders");
            Create(assetsFolderStructureEntry, "Scripts/Tests/Editor");
            Create(assetsFolderStructureEntry, "Scripts/Tests/Runtime");
            Create(assetsFolderStructureEntry, "Textures");

            return assetsFolderStructureEntry;
        }


        public FolderStructureEntry(string name, FolderStructureEntry parent)
        {
            Name = name;
            _children = new List<FolderStructureEntry>();
            Parent = parent;
        }


        [JsonConstructor]
        public FolderStructureEntry(string name, List<FolderStructureEntry> children)
        {
            Name = name;

            if (children == null)
            {
                children = new List<FolderStructureEntry>();
            }
            
            _children = children;

            foreach (FolderStructureEntry child in children)
            {
                child.Parent = this;
            }
        }


        public FolderStructureEntry(string name, FolderStructureEntry parent, List<FolderStructureEntry> children)
        {
            _children = children;
            Name = name;
            Parent = parent;

            children = _children.Distinct().ToList();
            
            foreach (FolderStructureEntry child in children)
            {
                child.Parent = this;
            }
        }


        public List<FolderStructureEntry> GetChildren()
        {
            return new List<FolderStructureEntry>(_children);
        }


        public void AddChild(FolderStructureEntry folderStructureEntry)
        {
            if (!_children.Contains(folderStructureEntry))
            {
                folderStructureEntry.Parent = this;
                _children.Add(folderStructureEntry);
                _children = _children.OrderBy(c => c.Name).ToList();
                //Debug.Log($"Added child: {folderStructureEntry.Name}");
            }
        }


        public void RemoveChild(FolderStructureEntry folderStructureEntry)
        {
            if (_children.Contains(folderStructureEntry))
            {
                _children.Remove(folderStructureEntry);
            }
        }


        public string[] ToFolderNames(bool includeRoot = false)
        {
            List<string> names = new List<string>();

            if (includeRoot)
            {
                names.Add(FullName);
            }

            foreach (FolderStructureEntry child in _children)
            {
                names.AddRange(child.ToFolderNames(true));
            }

            return names.ToArray();
        }


        /// <summary>
        /// Renames the folder.
        /// </summary>
        /// <param name="newName">Expects valid name.</param>
        public void Rename(string newName)
        {
            Name = newName;
        }


        /// <summary>
        /// Creates folder along with all the parent folders in the path
        /// </summary>
        /// <param name="path">Path relative to the root folder</param>
        /// <returns>The topmost parent folder</returns>
        public static void Create(FolderStructureEntry root, string path)
        {
            string[] folders = path.Split('/');
            FolderStructureEntry current = root;

            for (int i = 0; i < folders.Length; i++)
            {
                FolderStructureEntry child = new FolderStructureEntry(folders[i], current);
                if (!current._children.Contains(child))
                {
                    current.AddChild(child);
                    current = child;
                }
                else
                {
                    current = current._children.Find(c => c.Equals(child));
                }
            }
        }


        public bool Equals(FolderStructureEntry other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return FullName == other.FullName;
        }


        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((FolderStructureEntry) obj);
        }


        public override int GetHashCode()
        {
            return (FullName != null ? FullName.GetHashCode() : 0);
        }
    }
}