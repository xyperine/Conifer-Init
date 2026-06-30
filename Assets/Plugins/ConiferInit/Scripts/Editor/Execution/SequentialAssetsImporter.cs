using System;
using System.Collections.Generic;
using System.Linq;
using ConiferInit.Editor.Configuration;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ConiferInit.Editor.Execution
{
    /// <summary>
    /// Handles sequential interactive assets import.
    /// </summary>
    internal static class SequentialAssetsImporter
    {
        private static ExecutionCache _data;


        [InitializeOnLoadMethod]
        private static void SubscribeToEvents()
        {
            _data = ExecutionCache.instance;

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
            
            _data = ExecutionCache.instance;
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
        }


        private static void Update()
        {
            if (!_data.Stable)
            {
                return;
            }
            
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || AssetDatabase.IsAssetImportWorkerProcess())
            {
                return;
            }
            
            // Halt the importer if there are no more assets to import
            if (_data.AssetsToImport.Count == 0)
            {
                End();
            
                return;
            }
        
            Import();
        }


        private static void Import()
        {
            if (_data.Importing)
            {
                return;
            }
            
            _data.Importing = true;

            AssetImportEntry asset = _data.AssetsToImport.First();
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
            
            Assert.IsTrue(asset.Path.EndsWith(UnityPackageUtility.UNITY_PACKAGE_FILE_EXTENSION));
            
            // Because different asset import events return different strings🤪
            return asset.Name == packageName || asset.Path ==
                packageName + UnityPackageUtility.UNITY_PACKAGE_FILE_EXTENSION;
        }
    }
}