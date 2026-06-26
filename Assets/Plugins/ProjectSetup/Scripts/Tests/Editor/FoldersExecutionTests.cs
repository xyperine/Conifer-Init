using System;
using System.IO;
using NUnit.Framework;
using ProjectSetupTool.Editor.Execution;
using UnityEditor;
using UnityEngine;

namespace ProjectSetupTool.Editor.Tests
{
    internal sealed class FoldersExecutionTests
    {
        private readonly string[] _folderNames = 
        {
            "Animations",
            "Audio",
            "Data",
            "Data/Inputs",
            "Data/URP",
            "Materials",
            "Meshes",
            "Prefabs",
            "Shaders",
            "Scripts",
            "Scripts/Tests",
            "Scripts/Tests/Editor",
            "Scripts/Tests/Runtime",
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
                "Data/Inputs",
                "Data/URP",
                "Scripts/Tests/Editor",
                "Scripts/Tests/Runtime",
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
            string[] folderNames = {"||||||"};
            
            // "Act"
            TestDelegate action = () => Folders.Create(string.Empty, folderNames);
            
            // Assert
            Assert.Throws<ArgumentException>(action);
        }


        [TearDown]
        public void CleanUp()
        {
            foreach (string folderName in _folderNames)
            {
                string path = $"Assets/{folderName}";
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.DeleteAsset(path + ".meta");
            }
        }
    }
}