using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ConiferInit.Editor.Execution
{
    internal static class Packages
    {
        private static AddAndRemoveRequest _request;
        
        
        public static void ImportMany(IEnumerable<string> packages)
        {
            //Assert
            if (!packages.Any())
            {
                Debug.LogError("Empty package list!");
                return;
            }

            Debug.Log("Importing packages...");
            _request = Client.AddAndRemove(packages.ToArray(), new string[] { });
            
            EditorUtility.DisplayProgressBar("Importing Packages", "Preparing...", 0.1f);
            
            EditorApplication.update += Update;
        }


        private static void Update()
        {
            if (_request == null)
            {
                return;
            }
            
            if (!_request.IsCompleted)
            {
                return;
            }
            
            EditorUtility.ClearProgressBar();

            EditorApplication.update -= Update;
            
            ExecutionCache.instance.NonInteractiveOperationsInProgress = false;
            ExecutionCache.instance.NonInteractiveOperationsFinished = true;
            ExecutionCache.instance.PackagesToImport = new List<string>();

            switch (_request.Status)
            {
                case StatusCode.InProgress:
                    Debug.LogError("Invalid status!");
                    break;
                case StatusCode.Success:
                    Debug.Log("Imported packages, including dependencies:");
                    foreach (PackageInfo packageInfo in _request.Result)
                    {
                        Debug.Log($"Successfully imported {packageInfo.displayName}.");
                    }
                    break;
                case StatusCode.Failure:
                    Debug.LogError(_request.Error.message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}