using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ProjectSetupTool.Editor.Configuration
{
    /// <summary>
    /// Describes a folder in the Assets directory. Paths are relative to the Assets directory. Implements a tree structure.
    /// </summary>
    [Serializable]
    internal sealed class FolderStructureEntry : IEquatable<FolderStructureEntry>
    {
        [field: JsonProperty, SerializeReference] public List<FolderStructureEntry> Children { get; private set; }
            
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeReference] public FolderStructureEntry Parent { get; private set; }
        
        public string FullName =>
            Path.Combine(Parent == null || Parent.FullName == "Assets" 
                ? string.Empty 
                : Parent.FullName
                , Name);


        public FolderStructureEntry(string name, FolderStructureEntry parent)
        {
            Name = name;
            Parent = parent;
            Children = new List<FolderStructureEntry>();
        }


        [JsonConstructor]
        public FolderStructureEntry(string name, List<FolderStructureEntry> children)
        {
            Name = name;

            if (children == null)
            {
                children = new List<FolderStructureEntry>();
            }
            
            Children = children;

            foreach (FolderStructureEntry child in children)
            {
                child.Parent = this;
            }
        }


        public FolderStructureEntry(string name, FolderStructureEntry parent, List<FolderStructureEntry> children)
        {
            Name = name;
            Parent = parent;
            Children = children;

            children = Children.Distinct().ToList();
            
            foreach (FolderStructureEntry child in children)
            {
                child.Parent = this;
            }
        }


        public void AddChild(FolderStructureEntry folderStructureEntry)
        {
            if (!Children.Contains(folderStructureEntry))
            {
                folderStructureEntry.Parent = this;
                Children.Add(folderStructureEntry);
                Children = Children.OrderBy(c => c.Name).ToList();
            }
        }


        public void RemoveChild(FolderStructureEntry folderStructureEntry)
        {
            if (Children.Contains(folderStructureEntry))
            {
                Children.Remove(folderStructureEntry);
            }
        }


        /// <summary>
        /// Converts the entire structure into a list of paths.
        /// </summary>
        /// <param name="includeRoot">Include the root folder in the list?</param>
        /// <returns>A list of paths relative to the Assets directory.</returns>
        public string[] ToFolderNames(bool includeRoot = false)
        {
            List<string> names = new List<string>();

            if (includeRoot)
            {
                names.Add(FullName);
            }

            foreach (FolderStructureEntry child in Children)
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


        /// <summary>
        /// Creates a default configuration for FolderStructureEntry.
        /// </summary>
        /// <returns>Root of the created default structure.</returns>
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


        /// <summary>
        /// Creates a deep copy of the given entry.
        /// </summary>
        /// <param name="original">Entry to copy.</param>
        /// <param name="parent">Parent of the passed entry.</param>
        /// <returns>Copied entry.</returns>
        public static FolderStructureEntry DeepCopy(FolderStructureEntry original, FolderStructureEntry parent)
        {
            string name = original.Name;
            List<FolderStructureEntry> children = new List<FolderStructureEntry>();
            FolderStructureEntry copy = new FolderStructureEntry(name, parent, children);
            
            foreach (FolderStructureEntry child in original.Children)
            {
                children.Add(DeepCopy(child, copy));
            }

            return copy;
        }


        /// <summary>
        /// Creates folder along with all the parent folders in the path.
        /// </summary>
        /// <param name="path">Path relative to the root folder.</param>
        /// <returns>The topmost parent folder.</returns>
        public static void Create(FolderStructureEntry root, string path)
        {
            string[] folders = path.Split('/');
            FolderStructureEntry current = root;

            for (int i = 0; i < folders.Length; i++)
            {
                FolderStructureEntry child = new FolderStructureEntry(folders[i], current);
                if (!current.Children.Contains(child))
                {
                    current.AddChild(child);
                    current = child;
                }
                else
                {
                    current = current.Children.Find(c => c.Equals(child));
                }
            }
        }
    }
}