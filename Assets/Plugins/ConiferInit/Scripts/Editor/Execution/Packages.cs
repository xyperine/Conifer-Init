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
        private static AddRequest _request;
        private static readonly Queue<string> PackagesToInstall = new Queue<string>();
            
            
        public static async Task ImportAsync(IEnumerable<string> packages)
        {
            //Assert
            if (!packages.Any())
            {
                Debug.LogError("Empty package list!");
                return;
            }
            
            foreach (string package in packages)
            {
                PackagesToInstall.Enqueue(package);
            }

            while (PackagesToInstall.Count >= 1)
            {
                await ImportAsync(PackagesToInstall.Dequeue());
                await Task.Delay(1000);
            }
            
            Debug.Log("All packages imported!");
        }


        public static async Task ImportAsync(string package)
        {
            _request = Client.Add(package);

            while (!_request.IsCompleted)
            {
                await Task.Delay(10);
            }

            switch (_request.Status)
            {
                case StatusCode.InProgress:
                    Debug.LogError("The import is considered to be in progress!");
                    break;
                case StatusCode.Success:
                    Debug.Log($"Successfully installed: {_request.Result.packageId}");
                    break;
                case StatusCode.Failure:
                    Debug.LogError(_request.Error.message);
                    break;
                default:
                    Debug.LogError("Invalid status!");
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}