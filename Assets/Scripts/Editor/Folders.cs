using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor
{
    // TODO: Create assembly definition files
    internal static class Folders
    {
        public static void Create(string destination, string[] folders)
        {
            string fullPath = Path.Combine(Application.dataPath, destination);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
                
            foreach (string folder in folders)
            {
                CreateSubFolders(fullPath, folder);
            }
                
        }
            
            
        static void CreateSubFolders(string rootPath, string folderHierarchy) 
        {
            string[] folders = folderHierarchy.Split('/');
            string currentPath = rootPath;

            foreach (string folder in folders) 
            {
                currentPath = Path.Combine(currentPath, folder);
                if (!Directory.Exists(currentPath))
                {
                    Directory.CreateDirectory(currentPath);
                }
            }
        }
            
            
        public static void Delete(string folderName)
        {
            string path = $"Assets/{folderName}";
            if (AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
        }


        // TODO: Consider checking with AssetDatabase.ValidateMoveAsset()
        public static void Move(string newParent, string folderName)
        {
            string sourcePath = $"Assets/{folderName}";
            if (AssetDatabase.IsValidFolder(sourcePath))
            {
                string destinationPath = $"Assets/{newParent}/{folderName}";
                string moveResult = AssetDatabase.MoveAsset(sourcePath, destinationPath);
                bool movedSuccessfully = string.IsNullOrEmpty(moveResult);
                if (!movedSuccessfully)
                {
                    Debug.LogError($"Failed to move {folderName} under {newParent}: {moveResult}");
                }
            }
        }
    }
}