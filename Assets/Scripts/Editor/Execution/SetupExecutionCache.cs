using System.Collections.Generic;
using ProjectSetup.Editor.Configuration;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor.Execution
{
    /// <summary>
    /// Contains tool data that exists only during the setup process and needs to survive domain reloads.
    /// </summary>
    internal sealed class SetupExecutionCache : ScriptableSingleton<SetupExecutionCache>
    {
        [field: SerializeField] public bool PreInteractiveOperationsInProgress { get; set; }
        [field: SerializeField] public bool PreInteractiveOperationsFinished { get; set; }
        
        [field: SerializeField] public bool InteractiveOperationsInProgress { get; set; }
        [field: SerializeField] public List<AssetImportEntry> AssetsToImport { get; set; }
        [field: SerializeField] public bool SetupInProgress { get; set; }
        [field: SerializeField] public bool Importing { get; set; }
        [field: SerializeField] public bool Stable { get; set; }
        [field: SerializeField] public bool ImportRequested { get; set; }
        [field: SerializeField] public bool InteractiveOperationsFinished { get; set; }
        
        [field: SerializeField] public bool NonInteractiveOperationsInProgress { get; set; }
        [field: SerializeField] public bool NonInteractiveOperationsFinished { get; set; }

        public bool AllSetupStagesComplete => PreInteractiveOperationsFinished && InteractiveOperationsFinished &&
                                              NonInteractiveOperationsFinished;
        
        
        public void ResetSetup()
        {
            SetupInProgress = false;
            PreInteractiveOperationsInProgress = false;
            PreInteractiveOperationsFinished = false;
            InteractiveOperationsInProgress = false;
            AssetsToImport.Clear();
            Importing = false;
            Stable = true;
            ImportRequested = false;
            InteractiveOperationsFinished = false;
            NonInteractiveOperationsInProgress = false;
            NonInteractiveOperationsFinished = false;
        }
    }
}