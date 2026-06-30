using System.IO;
using UnityEngine;

namespace ConiferInit.Editor.Execution
{
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
    }
}