using System.Collections;
using System.IO;
using ConiferInit.Editor.Execution;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace ConiferInit.Editor.Tests.Execution
{
    internal sealed class AssetsTests
    {
        private readonly string[] _assets =
        {
            "Test_Asset_1.unitypackage",
            "Test_Asset_2.unitypackage",
            "Test_Asset_3.unitypackage",
        };

        private readonly string[] _importedFiles =
        {
            "Assets/Test1.txt",
            "Assets/Test2.txt",
            "Assets/Test3.txt",
        };

        private readonly string _pathToTestAssets =
            Path.Combine(Application.dataPath, "Plugins", "ConiferInit", "Scripts", "Tests", "Editor");
        
        
        [UnityTest]
        public IEnumerator Specified_assets_that_dont_trigger_domain_reload_were_imported_successfully()
        {
            // Act
            foreach (string asset in _assets)
            {
                string fullPath = Path.Combine(_pathToTestAssets, asset);
                Assets.Import(fullPath);
            }

            yield return new WaitForSecondsRealtime(0.5f);
            
            // Assert
            foreach (string importedFile in _importedFiles)
            {
                Assert.IsTrue(AssetDatabase.AssetPathExists(importedFile));
            }
        }
        
        
        [UnityTest]
        public IEnumerator Correctly_identifies_all_package_assets_within_project()
        {
            // Arrange
            string[] assetsToImport = _assets[..2];
            string assetNotToImport = _assets[^1];
            
            // Act
            foreach (string asset in assetsToImport)
            {
                string fullPath = Path.Combine(_pathToTestAssets, asset);
                Assets.Import(fullPath);
            }

            yield return new WaitForSecondsRealtime(0.5f);
            
            // Assert
            foreach (string asset in assetsToImport)
            {
                string fullPath = Path.Combine(_pathToTestAssets, asset);
                Assert.IsTrue(UnityPackageUtility.AllPluginAssetsAlreadyImported(fullPath));
            }

            string notImportedAssetFullPath = Path.Combine(_pathToTestAssets, assetNotToImport);
            Assert.IsFalse(UnityPackageUtility.AllPluginAssetsAlreadyImported(notImportedAssetFullPath));
        }


        [TearDown]
        public void CleanUp()
        {
            foreach (string importedFile in _importedFiles)
            {
                AssetDatabase.DeleteAsset(importedFile);
            }
        }
    }
}