using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Handles sequential interactive assets import.
    /// </summary>
    internal static class SequentialAssetsImporter
    {
        private static ProjectSetupData _data;


        [InitializeOnLoadMethod]
        private static void SubscribeToEvents()
        {
            _data = ProjectSetupData.instance;

            if (!_data.ImportRequested)
            {
                return;
            }
            
            AssetDatabase.importPackageCompleted += OnAssetImported;
            AssetDatabase.importPackageCancelled += OnAssetImportCancelled;
            AssetDatabase.importPackageFailed += OnAssetImportFailed;
            EditorApplication.update += Update;
        }
        
        
        public static void Begin(IEnumerable<AssetImportEntry> assets)
        {
            Assert.IsTrue(assets.Any());
            
            _data = ProjectSetupData.instance;
            _data.AssetsToImport = assets.ToList();
            _data.ImportRequested = true;
            _data.InteractiveOperationsFinished = false;
            
            _data.Importing = false;

            WaitForStability();
            
            SubscribeToEvents();
        }


        private static void WaitForStability()
        {
            _data.Stable = false;
            EditorApplication.delayCall += SetStable;
        }


        private static void SetStable()
        {
            _data.Stable = true;
        }


        public static void End()
        {
            _data.ImportRequested = false;
            
            AssetDatabase.importPackageCompleted -= OnAssetImported;
            AssetDatabase.importPackageCancelled -= OnAssetImportCancelled;
            AssetDatabase.importPackageFailed -= OnAssetImportFailed;
            
            EditorApplication.update -= Update;

            _data.InteractiveOperationsFinished = true;
            
            Debug.Log("All assets imported!");
        }


        private static void Update()
        {
            if (!_data.Stable)
            {
                Debug.Log("Unstable");
                return;
            }
            
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || AssetDatabase.IsAssetImportWorkerProcess())
            {
                Debug.Log("Working");
                return;
            }
            
            // Halt the importer if there are no more assets to import
            if (_data.AssetsToImport.Count == 0)
            {
                Debug.Log("Nothing to import");
                
                End();
            
                return;
            }
        
            Import();
        }


        private static void Import()
        {
            if (_data.Importing)
            {
                Debug.Log("Already importing");
                
                return;
            }
            
            _data.Importing = true;

            AssetImportEntry asset = _data.AssetsToImport.First();
            Debug.Log($"Importing: {asset.Name}");
            try
            {
                if (UnityPackageUtility.AllPluginAssetsAlreadyImported(asset.Path))
                {
                    _data.Importing = false;
                    _data.AssetsToImport.RemoveAt(0);
                
                    Debug.Log($"{asset.Name} is fully imported already");
                
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                
                _data.Importing = false;
                _data.AssetsToImport.RemoveAt(0);
                
                return;
            }
            
            AssetDatabase.ImportPackage(asset.Path, asset.Interactive);
        }


        private static void OnAssetImportCancelled(string packageName)
        {
            if (!IsTheQueuedPackage(packageName))
            {
                return;
            }
            
            Debug.Log($"Canceled: {packageName}");
            
            _data.Importing = false;
            _data.AssetsToImport.RemoveAt(0);
            
            WaitForStability();
        }


        private static void OnAssetImported(string packageName)
        {
            if (!IsTheQueuedPackage(packageName))
            {
                return;
            }
            
            Debug.Log($"Imported: {packageName}");
            
            _data.Importing = false;
            _data.AssetsToImport.RemoveAt(0);
            
            WaitForStability();
        }


        private static void OnAssetImportFailed(string packageName, string errorMessage)
        {
            if (!IsTheQueuedPackage(packageName))
            {
                return;
            }
            
            Debug.LogError(errorMessage);

            _data.Importing = false;
            _data.AssetsToImport.RemoveAt(0);

            WaitForStability();
        }


        private static bool IsTheQueuedPackage(string packageName)
        {
            AssetImportEntry asset = _data.AssetsToImport.First();
            
            //Debug.Log(packageName);
            //Debug.Log(asset.Path);
            
            Assert.IsTrue(asset.Path.EndsWith(UnityPackageUtility.UNITY_PACKAGE_FILE_EXTENSION));
            
            // Because different asset import events return different strings🤪
            return asset.Name == packageName || asset.Path ==
                packageName + UnityPackageUtility.UNITY_PACKAGE_FILE_EXTENSION;
        }
    }
}