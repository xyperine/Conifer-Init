using System.Linq;
using ConiferInit.Editor.Configuration;
using NUnit.Framework;

namespace ConiferInit.Editor.Tests.Configuration
{
    internal sealed class FolderStructureEntryTests
    {
        private readonly string[] _folderNames = 
        {
            "Animations",
            "Audio",
            "Data",
            "Data\\Inputs",
            "Data\\URP",
            "Materials",
            "Meshes",
            "Plugins",
            "Prefabs",
            "Shaders",
            "Scripts",
            "Scripts\\Tests",
            "Scripts\\Tests\\Editor",
            "Scripts\\Tests\\Runtime",
            "Textures",
        };
        

        [Test]
        public void Creates_valid_hierarchy_from_full_paths()
        {
            // Arrange
            string[] fullPaths =
            {
                "A/B/C/D",
                "E/F/G",
            };
            
            // Act
            FolderStructureEntry entry = CreateTestEntry(fullPaths);
            
            // Assert
            string[] expectedFolderNames =
            {
                "A",
                @"A\B",
                @"A\B\C",
                @"A\B\C\D",
                "E",
                @"E\F",
                @"E\F\G",
            };
            string[] actual = entry.ToFolderNames().OrderBy(n => n).ToArray();
            Assert.IsTrue(expectedFolderNames.SequenceEqual(actual));
        }
        
        
        [Test]
        public void Creates_accurate_deep_copy()
        {
            // Arrange
            FolderStructureEntry entry = CreateTestEntry(_folderNames);
            
            // Act
            FolderStructureEntry copy = FolderStructureEntry.DeepCopy(entry, null);
            
            // Assert
            string[] expected = _folderNames.OrderBy(n => n).ToArray();
            string[] actual = copy.ToFolderNames().OrderBy(n => n).ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }


        private FolderStructureEntry CreateTestEntry(string[] folderNames)
        {
            FolderStructureEntry assetsFolderStructureEntry =
                new FolderStructureEntry("Assets", (FolderStructureEntry) null);

            foreach (string folderName in folderNames)
            {
                FolderStructureEntry.Create(assetsFolderStructureEntry, folderName);
            }

            return assetsFolderStructureEntry;
        }
    }
}