using System.IO;
using ConiferInit.Editor.Execution;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor.Tests.Execution
{
    internal sealed class FoldersExecutionTests
    {
        private readonly string[] _folderNames = 
        {
            "Animations",
            "Audio",
            "Data",
            Path.Combine("Data", "Inputs"),
            Path.Combine("Data", "URP"),
            "Materials",
            "Meshes",
            "Prefabs",
            "Shaders",
            "Scripts",
            Path.Combine("Scripts", "Tests"),
            Path.Combine("Scripts", "Tests", "Editor"),
            Path.Combine("Scripts", "Tests", "Runtime"),
            "Textures",
        };
        
        
        [Test]
        public void All_specified_folders_are_created_in_assets_directory()
        {
            // Act
            Folders.Create(string.Empty, _folderNames);

            // Assert
            foreach (string folderName in _folderNames)
            {
                string path = Path.Combine(Application.dataPath, folderName);
                Assert.IsTrue(Directory.Exists(path), folderName);
            }
        }


        [Test]
        public void Successfully_created_subdirectories()
        {
            // Act
            Folders.Create(string.Empty, _folderNames);

            string[] expectedSubdirectories =
            {
                Path.Combine("Data", "Inputs"),
                Path.Combine("Data", "URP"),
                Path.Combine("Scripts", "Tests", "Editor"),
                Path.Combine("Scripts", "Tests", "Runtime"),
            };
            
            // Assert
            foreach (string folderName in expectedSubdirectories)
            {
                string path = Path.Combine(Application.dataPath, folderName);
                Assert.IsTrue(Directory.Exists(path), folderName);
            }
        }


        [Test]
        public void Cannot_create_directory_with_invalid_name()
        {
            // Arrange
            string[] folderNames = {"///////", "///", "/", "////////////////"};
            
            // "Act"
            TestDelegate action = () => Folders.Create(string.Empty, folderNames);
            
            // Assert
            Assert.Throws<UnityEngine.Assertions.AssertionException>(action);
        }


        [TearDown]
        public void CleanUp()
        {
            foreach (string folderName in _folderNames)
            {
                string path = Path.Combine("Assets", folderName);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.DeleteAsset(path + ".meta");
            }
        }
    }
}