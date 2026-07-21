using System.IO;
using System.Text.RegularExpressions;
using ConiferInit.Editor.Configuration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ConiferInit.Editor.Tests
{
    internal sealed class SettingsProfileConfigurationTests
    {
        [SetUp]
        public void Setup()
        {
            ConfigurationCache.instance.Clear();
        }


        [Test]
        public void Identifies_valid_path_correctly()
        {
            // Arrange
            SettingsProfileConfiguration sut = new SettingsProfileConfiguration(ConfigurationCache.instance);
            string path = Path.Combine(SettingsProfilePersistency.StoragePath, "Test_Profile.json");

            // Act
            bool result = sut.IsValidProfilePath(path);

            // Assert
            LogAssert.NoUnexpectedReceived();
            Assert.IsTrue(result);
        }
        
        
        [Test]
        public void Identifies_path_outside_the_storage_as_invalid()
        {
            // Arrange
            SettingsProfileConfiguration sut = new SettingsProfileConfiguration(ConfigurationCache.instance);
            string path = Path.Combine(SettingsProfilePersistency.StoragePath.Replace("Profiles", "SomethingElse"),
                "Profile.json");

            // Act
            bool result = sut.IsValidProfilePath(path);

            // Assert
            LogAssert.Expect(LogType.Error, new Regex(".*storage.*"));
            Assert.IsFalse(result);
        }
        
        
        [Test]
        public void Identifies_path_with_wrong_extension_as_invalid()
        {
            // Arrange
            SettingsProfileConfiguration sut = new SettingsProfileConfiguration(ConfigurationCache.instance);
            string path = Path.Combine(SettingsProfilePersistency.StoragePath, "Profile.yml");

            // Act
            bool result = sut.IsValidProfilePath(path);

            // Assert
            LogAssert.Expect(LogType.Error, new Regex(".*extension.*"));
            Assert.IsFalse(result);
        }
        
        
        [Test]
        public void Identifies_path_to_default_profile_as_invalid()
        {
            // Arrange
            SettingsProfileConfiguration sut = new SettingsProfileConfiguration(ConfigurationCache.instance);
            string path = Path.Combine(SettingsProfilePersistency.StoragePath, "Default_Profile.json");

            // Act
            bool result = sut.IsValidProfilePath(path);

            // Assert
            LogAssert.Expect(LogType.Error, new Regex(".*default profile.*"));
            Assert.IsFalse(result);
        }
        
        
        [Test]
        public void Identifies_valid_path_for_new_profile_correctly()
        {
            // Arrange
            SettingsProfileConfiguration sut = new SettingsProfileConfiguration(ConfigurationCache.instance);
            string path = Path.Combine(SettingsProfilePersistency.StoragePath, "Test_Profile.json");

            // Act
            bool result = sut.IsValidNewProfilePath(path);

            // Assert
            LogAssert.NoUnexpectedReceived();
            Assert.IsTrue(result);
        }
        
        
        [Test]
        public void Identifies_path_for_new_profile_outside_the_storage_as_invalid()
        {
            // Arrange
            SettingsProfileConfiguration sut = new SettingsProfileConfiguration(ConfigurationCache.instance);
            string path = Path.Combine(SettingsProfilePersistency.StoragePath.Replace("Profiles", "SomethingElse"),
                "Profile.json");

            // Act
            bool result = sut.IsValidNewProfilePath(path);

            // Assert
            LogAssert.Expect(LogType.Error, new Regex(".*storage.*"));
            Assert.IsFalse(result);
        }
        
        
        [Test]
        public void Identifies_path_for_new_profile_with_wrong_extension_as_invalid()
        {
            // Arrange
            SettingsProfileConfiguration sut = new SettingsProfileConfiguration(ConfigurationCache.instance);
            string path = Path.Combine(SettingsProfilePersistency.StoragePath, "Profile.yml");

            // Act
            bool result = sut.IsValidNewProfilePath(path);

            // Assert
            LogAssert.Expect(LogType.Error, new Regex(".*extension.*"));
            Assert.IsFalse(result);
        }
        
        
        [Test]
        public void Identifies_path_for_new_profile_that_overrides_existing_profile_as_invalid()
        {
            // Arrange
            SettingsProfileConfiguration sut = new SettingsProfileConfiguration(ConfigurationCache.instance);
            string path = Path.Combine(SettingsProfilePersistency.StoragePath, "Default_Profile.json");

            // Act
            bool result = sut.IsValidNewProfilePath(path);

            // Assert
            LogAssert.Expect(LogType.Error, new Regex(".*override.*"));
            Assert.IsFalse(result);
        }
        
        
        [OneTimeTearDown]
        public void CleanUp()
        {
            ConfigurationCache.instance.Clear();
        }
    }
}