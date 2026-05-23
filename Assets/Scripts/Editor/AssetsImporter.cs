using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Handles sequential interactive assets import
    /// </summary>
    public static class AssetsImporter
    {
        private static bool _isStable;
        private static bool _isImporting;
        private static Queue<AssetInfo> _assets;
        
        
        public static void Begin(IEnumerable<AssetInfo> assets)
        {
            _isStable = false;
            _isImporting = false;
            
            EditorApplication.delayCall += WaitForStability;

            _assets = new Queue<AssetInfo>(assets);
            
            AssetDatabase.importPackageCompleted += OnAssetImported;
            AssetDatabase.importPackageCancelled += OnAssetImportCancelled;
            AssetDatabase.importPackageFailed += OnAssetImportFailed;
            
            EditorApplication.update += Update;
        }


        private static void WaitForStability()
        {
            _isStable = true;
        }


        public static void End()
        {
            AssetDatabase.importPackageCompleted -= OnAssetImported;
            AssetDatabase.importPackageCancelled -= OnAssetImportCancelled;
            AssetDatabase.importPackageFailed -= OnAssetImportFailed;
            
            EditorApplication.update -= Update;
            
            Debug.Log("All assets imported!");
        }


        private static void Update()
        {
            if (!_isStable)
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
            if (_assets.Count == 0)
            {
                Debug.Log("Nothing to import");
                
                End();
            
                return;
            }
        
            Import();
        }


        private static void Import()
        {
            if (_isImporting)
            {
                Debug.Log("Already importing");
                
                return;
            }
            
            _isImporting = true;

            AssetInfo asset = _assets.Peek();
            Debug.Log($"Importing: {asset.Name}");
            try
            {
                if (UnityPackageUtility.AllPluginAssetsAlreadyImported(asset.Path))
                {
                    _isImporting = false;
                    _assets.Dequeue();
                
                    Debug.Log($"{asset.Name} is fully imported already");
                
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                
                _isImporting = false;
                _assets.Dequeue();
                
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
            
            _isImporting = false;
            _assets.Dequeue();
            
            _isStable = false;
            
            EditorApplication.delayCall += WaitForStability;
        }


        private static void OnAssetImported(string packageName)
        {
            if (!IsTheQueuedPackage(packageName))
            {
                return;
            }
            
            Debug.Log($"Imported: {packageName}");
            
            _isImporting = false;
            _assets.Dequeue();
            
            _isStable = false;
            
            EditorApplication.delayCall += WaitForStability;
        }


        private static void OnAssetImportFailed(string packageName, string errorMessage)
        {
            if (!IsTheQueuedPackage(packageName))
            {
                return;
            }
            
            Debug.LogError(errorMessage);

            _isImporting = false;
            _assets.Dequeue();

            _isStable = false;
            
            EditorApplication.delayCall += WaitForStability;
        }


        private static bool IsTheQueuedPackage(string packageName)
        {
            //Debug.Log(packageName);
            //Debug.Log(_assets.Peek().Path);

            var asset = _assets.Peek();
            
            Assert.IsTrue(asset.Path.EndsWith(UnityPackageUtility.UNITY_PACKAGE_FILE_EXTENSION));
            
            // Because different asset import events return different strings
            return asset.Name == packageName || asset.Path ==
                packageName + UnityPackageUtility.UNITY_PACKAGE_FILE_EXTENSION;
        }
    }
}