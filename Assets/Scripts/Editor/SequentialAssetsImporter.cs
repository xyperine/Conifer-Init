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
    public static class SequentialAssetsImporter
    {
        private static ProjectSetupData _data;


        [InitializeOnLoadMethod]
        private static void SubscribeToEvents()
        {
            _data = ProjectSetupData.instance;

            if (!_data.IsImportRequested)
            {
                return;
            }
            
            AssetDatabase.importPackageCompleted += OnAssetImported;
            AssetDatabase.importPackageCancelled += OnAssetImportCancelled;
            AssetDatabase.importPackageFailed += OnAssetImportFailed;
            
            EditorApplication.update += Update;
        }
        
        
        public static void Begin(IEnumerable<AssetInfo> assets)
        {
            Assert.IsTrue(assets.Any());
            
            _data = ProjectSetupData.instance;
            _data.AssetsToImport = assets.ToList();
            _data.IsImportRequested = true;
            
            _data.IsImporting = false;

            WaitForStability();
            
            SubscribeToEvents();
        }


        private static void WaitForStability()
        {
            _data.IsStable = false;
            EditorApplication.delayCall += SetStable;
        }


        private static void SetStable()
        {
            _data.IsStable = true;
        }


        public static void End()
        {
            _data.IsImportRequested = false;
            
            AssetDatabase.importPackageCompleted -= OnAssetImported;
            AssetDatabase.importPackageCancelled -= OnAssetImportCancelled;
            AssetDatabase.importPackageFailed -= OnAssetImportFailed;
            
            EditorApplication.update -= Update;
            
            Debug.Log("All assets imported!");
        }


        private static void Update()
        {
            if (!_data.IsStable)
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
            if (_data.IsImporting)
            {
                Debug.Log("Already importing");
                
                return;
            }
            
            _data.IsImporting = true;

            AssetInfo asset = _data.AssetsToImport.First();
            Debug.Log($"Importing: {asset.Name}");
            try
            {
                if (UnityPackageUtility.AllPluginAssetsAlreadyImported(asset.Path))
                {
                    _data.IsImporting = false;
                    _data.AssetsToImport.RemoveAt(0);
                
                    Debug.Log($"{asset.Name} is fully imported already");
                
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                
                _data.IsImporting = false;
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
            
            _data.IsImporting = false;
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
            
            _data.IsImporting = false;
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

            _data.IsImporting = false;
            _data.AssetsToImport.RemoveAt(0);

            WaitForStability();
        }


        private static bool IsTheQueuedPackage(string packageName)
        {
            AssetInfo asset = _data.AssetsToImport.First();
            
            //Debug.Log(packageName);
            //Debug.Log(asset.Path);
            
            Assert.IsTrue(asset.Path.EndsWith(UnityPackageUtility.UNITY_PACKAGE_FILE_EXTENSION));
            
            // Because different asset import events return different strings🤪
            return asset.Name == packageName || asset.Path ==
                packageName + UnityPackageUtility.UNITY_PACKAGE_FILE_EXTENSION;
        }
    }
}