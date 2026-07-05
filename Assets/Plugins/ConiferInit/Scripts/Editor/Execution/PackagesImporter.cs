using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Assertions;

namespace ConiferInit.Editor.Execution
{
    internal static class PackagesImporter
    { 
        private static ExecutionCache _data;


        [InitializeOnLoadMethod]
        private static void SubscribeToEvents()
        {
            _data = ExecutionCache.instance;

            if (!_data.NonInteractiveOperationsInProgress)
            {
                return;
            }
            
            EditorApplication.update += Update;
        }
        
        
        public static void Begin(IEnumerable<string> packages)
        {
            Assert.IsTrue(packages.Any());
            
            _data = ExecutionCache.instance;
            _data.PackagesToImport = new List<string>(packages);
            
            SubscribeToEvents();
        }


        public static void End()
        {
            EditorApplication.update -= Update;
        }


        private static void Update()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || AssetDatabase.IsAssetImportWorkerProcess())
            {
                return;
            }

            if (!_data.PackagesToImport.Any())
            {
                End();
                return;
            }
        
            Import();
        }


        private static void Import()
        {
            Packages.ImportMany(_data.PackagesToImport);
            End();
        }
    }
}