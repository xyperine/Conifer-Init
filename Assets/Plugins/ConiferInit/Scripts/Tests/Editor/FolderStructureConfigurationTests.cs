using System.Linq;
using ConiferInit.Editor.Configuration;
using NUnit.Framework;

namespace ConiferInit.Editor.Tests
{
    internal sealed class FolderStructureConfigurationTests
    {
        [SetUp]
        public void Setup()
        {
            ConfigurationCache.instance.Clear();
        }


        [Test]
        public void Removes_folder()
        {
            // Arrange
            SettingsProfileConfiguration profileConfiguration =
                new SettingsProfileConfiguration(ConfigurationCache.instance);
            FolderStructureConfiguration sut =
                new FolderStructureConfiguration(profileConfiguration, ConfigurationCache.instance);

            FolderStructureEntry parent = new FolderStructureEntry("Parent", (FolderStructureEntry) null);
            FolderStructureEntry child = new FolderStructureEntry("Child", parent);

            // Act
            sut.RemoveFolder(child);

            // Assert
            Assert.IsFalse(parent.Children.Contains(child));
        }
        
        
        [Test]
        public void Renames_folder()
        {
            // Arrange
            SettingsProfileConfiguration profileConfiguration =
                new SettingsProfileConfiguration(ConfigurationCache.instance);
            FolderStructureConfiguration sut =
                new FolderStructureConfiguration(profileConfiguration, ConfigurationCache.instance);

            FolderStructureEntry child = new FolderStructureEntry("Test", (FolderStructureEntry) null);
            string newName = "I am Renamed";
            
            // Act
            sut.RenameFolder(child, newName);

            // Assert
            string expected = "I am Renamed";
            Assert.AreEqual(expected, child.Name);
        }
        
        
        [Test]
        public void Adds_folder()
        {
            // Arrange
            SettingsProfileConfiguration profileConfiguration =
                new SettingsProfileConfiguration(ConfigurationCache.instance);
            FolderStructureConfiguration sut =
                new FolderStructureConfiguration(profileConfiguration, ConfigurationCache.instance);

            FolderStructureEntry parent = new FolderStructureEntry("Parent", (FolderStructureEntry) null);

            // Act
            sut.AddFolder("Child", parent);

            // Assert
            string expected = "Child";
            string[] actualChildrenNames = parent.Children.Select(c => c.Name).ToArray(); 
            Assert.Contains(expected, actualChildrenNames);
        }


        [OneTimeTearDown]
        public void CleanUp()
        {
            ConfigurationCache.instance.Clear();
        }
    }
}