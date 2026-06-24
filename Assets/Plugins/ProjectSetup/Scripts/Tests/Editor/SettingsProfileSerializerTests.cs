using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProjectSetupTool.Editor.Configuration;

namespace ProjectSetupTool.Editor.Tests
{
    public class SettingsProfileSerializerTests
    {
        private const string TEST_PROFILE_NAME = "Test";
        
        
        [Test]
        public void Saved_profile_deserializes_correctly()
        {
            // Arrange
            SettingsProfile profile = CreateTestProfile();
            string fileName = profile.Name + ".json";
            
            // Act
            SettingsProfileSerializer.SaveFile(profile, fileName);
            SettingsProfile actual = SettingsProfileSerializer.ReadFile(fileName);
            
            // Assert
            SettingsProfile expected = CreateTestProfile();
            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void Profile_file_is_saved_in_the_storage()
        {
            // Arrange
            SettingsProfile profile = CreateTestProfile();
            string fileName = profile.Name + ".json";
            
            // Act
            SettingsProfileSerializer.SaveFile(profile, fileName);
            
            // Assert
            string pathToFile = Path.Combine(SettingsProfileSerializer.ProfilesStoragePath, fileName);
            bool result = File.Exists(pathToFile);
            Assert.IsTrue(result);
        }


        [Test]
        public void Profile_deleted_from_the_storage()
        {
            // Arrange
            SettingsProfile profile = CreateTestProfile();
            string fileName = profile.Name + ".json";
            
            // Act
            SettingsProfileSerializer.SaveFile(profile, fileName);
            SettingsProfileSerializer.DeleteFile(fileName);
            
            // Assert
            string pathToFile = Path.Combine(SettingsProfileSerializer.ProfilesStoragePath, fileName);
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
            SettingsProfileSerializer.DeleteFile(TEST_PROFILE_NAME);
        }
    }
}
