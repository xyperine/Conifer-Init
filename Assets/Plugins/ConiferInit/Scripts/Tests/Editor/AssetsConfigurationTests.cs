using System;
using System.Linq;
using ConiferInit.Editor.Configuration;
using NUnit.Framework;
using UnityEngine;

namespace ConiferInit.Editor.Tests
{
    internal sealed class AssetsConfigurationTests
    {
        [SetUp]
        public void Setup()
        {
            ConfigurationCache.instance.Clear();
        }
        
        
        [Test]
        public void Cant_add_assets_that_are_not_in_the_cache()
        {
            // Arrange
            AssetsSettingsConfiguration sut = new AssetsSettingsConfiguration(ConfigurationCache.instance);
            string[] assetIDs = {"fdsagrag", "uy4y54", "7o87SDADAS"};

            // Act
            try
            {
                foreach (string assetID in assetIDs)
                {
                    sut.QueueAsset(assetID);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Caught exception: {e}");
            }

            // Assert
            Assert.IsEmpty(sut.GetQueuedAssets());
        }


        [OneTimeTearDown]
        public void CleanUp()
        {
            ConfigurationCache.instance.Clear();
        }
    }
}