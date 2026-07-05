using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ConiferInit.Initialization
{
    [InitializeOnLoad]
    internal static class Initialization
    {
        private const string NEWTONSOFT_JSON_ID = "com.unity.nuget.newtonsoft-json";
        
        private static readonly ListRequest ListRequest;
        
        
        static Initialization()
        {
            ListRequest = Client.List(true);
            
            EditorApplication.update += Update;
        }


        private static void Update()
        {
            if (ListRequest.IsCompleted)
            {
                switch (ListRequest.Status)
                {
                    case StatusCode.InProgress:
                        Debug.Log("Invalid request");
                        break;
                    case StatusCode.Success:
                        CheckForDependencies(ListRequest.Result);
                        break;
                    case StatusCode.Failure:
                        Debug.LogError(ListRequest.Error.message);
                        EditorApplication.update -= Update;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        private static void CheckForDependencies(PackageCollection collection)
        {
            bool alreadyInstalled = false;
            
            foreach (PackageInfo packageInfo in collection)
            {
                if (packageInfo.name == NEWTONSOFT_JSON_ID)
                {
                    alreadyInstalled = true;
                }
            }

            if (!alreadyInstalled)
            {
                InstallDependencies();
            }
            else
            {
                EditorApplication.update -= Update;
            }
        }
        

        private static void InstallDependencies()
        {
            Debug.Log("Installing dependencies...");
            Client.Add(NEWTONSOFT_JSON_ID);

            EditorApplication.update -= Update;
        }
    }
}