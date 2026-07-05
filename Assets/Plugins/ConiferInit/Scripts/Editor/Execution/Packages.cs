using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ConiferInit.Editor.Execution
{
    internal static class Packages
    {
        private static AddAndRemoveRequest _request;
        
            
        public static async Task ImportAsync(IEnumerable<string> packages)
        {
            //Assert
            if (!packages.Any())
            {
                Debug.LogError("Empty package list!");
                return;
            }

            await ImportManyAsync(packages);
        }


        private static async Task ImportManyAsync(IEnumerable<string> packages)
        {
            Debug.Log("Importing packages, please wait...");
            
            _request = Client.AddAndRemove(packages.ToArray(), new string[] { });

            while (!_request.IsCompleted)
            {
                await Task.Delay(10);
            }

            await Task.Delay(1000);

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