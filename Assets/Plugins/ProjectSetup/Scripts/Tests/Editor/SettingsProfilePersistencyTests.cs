using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProjectSetupTool.Editor.Configuration;

namespace ProjectSetupTool.Editor.Tests
{
    internal sealed class SettingsProfilePersistencyTests
    {
        private const string TEST_PROFILE_NAME = "Test";
        
        
        [Test]
        public void Saved_profile_restores_correctly()
        {
            // Arrange
            SettingsProfile profile = CreateTestProfile();
            string fileName = profile.Name + ".json";
            
            // Act
            SettingsProfilePersistency.Save(profile);
            SettingsProfile actual = SettingsProfilePersistency.Restore(fileName);
            
            // Assert
            SettingsProfile expected = CreateTestProfile();
            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void Profile_file_is_saved_in_the_storage()
        {
            // Arrange
            SettingsProfile profile = CreateTestProfile();
            
            // Act
            SettingsProfilePersistency.Save(profile);
            
            // Assert
            string fileName = profile.Name + ".json";
            string pathToFile = Path.Combine(SettingsProfilePersistency.StoragePath, fileName);
            bool result = File.Exists(pathToFile);
            Assert.IsTrue(result);
        }


        [Test]
        public void Profile_deleted_from_the_storage()
        {
            // Arrange
            SettingsProfile profile = CreateTestProfile();
            
            // Act
            SettingsProfilePersistency.Save(profile);
            SettingsProfilePersistency.Delete(profile);
            
            // Assert
            string fileName = profile.Name + ".json";
            string pathToFile = Path.Combine(SettingsProfilePersistency.StoragePath, fileName);
            bool result = File.Exists(pathToFile);
            Assert.IsFalse(result);
        }


        private SettingsProfile CreateTestProfile()
        {
            return new SettingsProfile()
            {
                AssetsFolderStructureEntry = FolderStructureEntry.Default(),
                MiscSettings = MiscSettings.Default(),
                Name = TEST_PROFILE_NAME,
                ProjectSettings = ProjectSettings.Default(),
                QueuedAssets = new List<AssetImportEntry>()
                {
                    new AssetImportEntry("some path", "some name", "some id", true),
                },
                QueuedPackages = new List<PackageImportEntry>()
                {
                    new PackageImportEntry("some short id", "some full id"),
                },
            };
        }
        
        
        [TearDown]
        public void CleanUp()
        {
            SettingsProfilePersistency.Delete(TEST_PROFILE_NAME);
        }
    }
}
