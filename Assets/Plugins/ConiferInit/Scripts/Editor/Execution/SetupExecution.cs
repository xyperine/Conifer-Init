using System;
using System.Collections.Generic;
using System.Linq;
using ConiferInit.Editor.Configuration;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ConiferInit.Editor.Execution
{
    /// <summary>
    /// Handles high-level logic of executing the setup by coordinating other components.
    /// </summary>
    internal sealed class SetupExecution
    {
        private ExecutionCache _executionCache;
        private ConfigurationCache _configurationCache;
        

        public void Initialize()
        {
            _executionCache = ExecutionCache.instance;
            _configurationCache = ConfigurationCache.instance;
        }
        
        
        public void ExecuteSetup()
        {
            _executionCache.SetupInProgress = true;
        }
        
        
        public void Update()
        {
            if (_executionCache.SetupInProgress)
            {
                PerformSetup();
            }
        }
        
        
        private void PerformSetup()
        {
            if (!_executionCache.SetupInProgress)
            {
                return;
            }

            if (!_executionCache.PreInteractiveOperationsInProgress && !_executionCache.PreInteractiveOperationsFinished)
            {
                _executionCache.PreInteractiveOperationsInProgress = true;
                
                //Debug.Log("Starting pre-interactive operations...");
                
                string[] folders = _configurationCache.AssetsFolderStructureEntry.ToFolderNames();
                CreateFolders(folders);

                _executionCache.PreInteractiveOperationsFinished = true;
                _executionCache.PreInteractiveOperationsInProgress = false;
            }

            if (!_executionCache.InteractiveOperationsInProgress && _executionCache.PreInteractiveOperationsFinished &&
                !_executionCache.InteractiveOperationsFinished)
            {
                _executionCache.InteractiveOperationsInProgress = true;
                
                //Debug.Log("Starting interactive operations...");

                IEnumerable<AssetImportEntry> assets = _configurationCache.QueuedAssets.Where(a => a.Interactive);
                if (assets.Any())
                {
                    ImportAssetsInteractive(assets);
                }
                else
                {
                    _executionCache.InteractiveOperationsFinished = true;
                    _executionCache.InteractiveOperationsInProgress = false;
                }
            }
                
            if (!_executionCache.NonInteractiveOperationsInProgress && _executionCache.InteractiveOperationsFinished &&
                _executionCache.PreInteractiveOperationsFinished && !_executionCache.NonInteractiveOperationsFinished)
            {
                _executionCache.NonInteractiveOperationsInProgress = true;
                
                //Debug.Log("Starting non-interactive operations...");

                IEnumerable<AssetImportEntry> assets =
                    _configurationCache.QueuedAssets.Where(a => !a.Interactive);
                if (assets.Any())
                {
                    Debug.Log("Importing assets...");
                    
                    ImportAssetsNonInteractive(assets);
                }
                
                ImportPackages(_configurationCache.QueuedPackages);
                    
                SetProjectSettings(_configurationCache.ProjectSettings);
                    
                ExecuteMisc(_configurationCache.MiscSettings);

                // Not really how it is supposed to work, as we need to actually wait for these operations to complete.
                //_executionCache.NonInteractiveOperationsInProgress = false;
                //_executionCache.NonInteractiveOperationsFinished = true;
            }

            if (_executionCache.AllSetupStagesComplete)
            {
                _executionCache.Clear();
                
                Debug.Log("Setup finished!");
            }
        }


        private void CreateFolders(string[] folders)
        {
            Folders.Create(string.Empty, folders);
            
            AssetDatabase.Refresh();
        }


        private void ImportAssetsInteractive(IEnumerable<AssetImportEntry> assets)
        {
            Assert.IsTrue(assets.All(a => a.Interactive));
            
            Assets.ImportInteractive(assets);
        }


        private void ImportAssetsNonInteractive(IEnumerable<AssetImportEntry> assets)
        {
            Assert.IsTrue(assets.Any());
            Assert.IsTrue(assets.All(a => !a.Interactive));
            
            foreach (AssetImportEntry asset in assets)
            {
                Assets.Import(asset.Path);
            }
        }


        private void ImportPackages(IEnumerable<PackageImportEntry> packages)
        {
            TMP_PackageResourceImporter.ImportResources(true, false, false);

            if (packages.Any())
            {
                PackagesImporter.Begin(packages.Select(p => p.FullID));
            }
            else
            {
                ExecutionCache.instance.NonInteractiveOperationsInProgress = false;
                ExecutionCache.instance.NonInteractiveOperationsFinished = true;
                ExecutionCache.instance.PackagesToImport = new List<string>();
            }
        }


        private void SetProjectSettings(ProjectSettings projectSettings)
        {
            ProjectSettingsExecution.Set(projectSettings);
        }


        private void ExecuteMisc(MiscSettings miscSettings)
        {
            MiscSettingsExecution.Execute(miscSettings);
        }
    }
}
