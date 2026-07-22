using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace ConiferInit.Editor.Execution
{
    internal static class Folders
    {
        /// <summary>
        /// Creates folder hierarchy from passed paths. Expects valid paths.
        /// </summary>
        /// <param name="destination">Root path relative to Assets.</param>
        /// <param name="folders">Folder paths relative to Assets.</param>
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
            rootPath = rootPath.Replace('/', Path.DirectorySeparatorChar);
            folderHierarchy = folderHierarchy.Replace('/', Path.DirectorySeparatorChar);
            
            string[] folders = folderHierarchy.Split(Path.DirectorySeparatorChar);
            string currentPath = rootPath;

            foreach (string folder in folders) 
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(folder), "Folder name can't be empty!");
                Assert.IsTrue(folder.IndexOfAny(Path.GetInvalidFileNameChars()) == -1,
                    $"Folder name contains invalid characters!: {folder}");
                
                currentPath = Path.Combine(currentPath, folder);
                if (!Directory.Exists(currentPath))
                {
                    Directory.CreateDirectory(currentPath);
                }
            }
        }
    }
}