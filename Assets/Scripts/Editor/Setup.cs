using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.SceneManagement;
using UnityEngine;
using Application = UnityEngine.Application;
using Object = UnityEngine.Object;

namespace ProjectSetup.Editor
{
    // TODO: Make it a separate window where you can customize the setup
    // TODO: Tweak project settings and preferences
    // TODO: Rename the sample scene to Main
    // TODO: Create Zenject project context and scene context if it is imported
    // TODO: Import my own helpers
    public static class Setup
    {
        [MenuItem("Tools/Setup/Folder Structure")]
        public static void FolderStructure()
        {
            CreateFolders();
            
            DeleteTutorialAssets();
            
            MoveDataAssets();
            
            SetupScene();
        }


        public static void CreateFolders(string[] folders)
        {
            Folders.Create(string.Empty, folders);
            
            AssetDatabase.Refresh();
        }


        private static void CreateFolders()
        {
            string[] folders =
            {
                "Audio", "Animations", "Data/Inputs", "Data/URP", "Meshes", "Textures", "Shaders", "Materials", "Plugins", "Prefabs",
                "Scripts/Tests/Editor", "Scripts/Tests/Runtime",
            };
            Folders.Create(string.Empty, folders);
            
            AssetDatabase.Refresh();
        }


        private static void DeleteTutorialAssets()
        {
            const string tutorialDirectory = "Assets/TutorialInfo";
            FileUtil.DeleteFileOrDirectory(tutorialDirectory);
            FileUtil.DeleteFileOrDirectory(tutorialDirectory + ".meta");
            const string readmeAssetPath = "Assets/Readme.asset";
            FileUtil.DeleteFileOrDirectory(readmeAssetPath + ".meta");
            FileUtil.DeleteFileOrDirectory(readmeAssetPath);
            
            AssetDatabase.Refresh();
        }
        

        private static void MoveDataAssets()
        {
            AssetDatabase.MoveAsset("Assets/InputSystem_Actions.inputactions", "Assets/Data/Inputs/InputSystem_Actions.inputactions");

            const string urpAssetsPath = "Assets/Data/URP";
            if (!AssetDatabase.IsValidFolder(urpAssetsPath))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, urpAssetsPath));
            }
            AssetDatabase.MoveAsset("Assets/Settings/DefaultVolumeProfile.asset", Path.Combine(urpAssetsPath, "DefaultVolumeProfile.asset"));
            AssetDatabase.MoveAsset("Assets/Settings/Mobile_Renderer.asset", Path.Combine(urpAssetsPath, "Mobile_Renderer.asset"));
            AssetDatabase.MoveAsset("Assets/Settings/Mobile_RPAsset.asset", Path.Combine(urpAssetsPath, "Mobile_RPAsset.asset"));
            AssetDatabase.MoveAsset("Assets/Settings/PC_Renderer.asset", Path.Combine(urpAssetsPath, "PC_Renderer.asset"));
            AssetDatabase.MoveAsset("Assets/Settings/PC_RPAsset.asset", Path.Combine(urpAssetsPath, "PC_RPAsset.asset"));
            AssetDatabase.MoveAsset("Assets/Settings/SampleSceneProfile.asset", Path.Combine(urpAssetsPath, "SampleSceneProfile.asset"));
            AssetDatabase.MoveAsset("Assets/Settings/UniversalRenderPipelineGlobalSettings.asset", Path.Combine(urpAssetsPath, "UniversalRenderPipelineGlobalSettings.asset"));
            
            AssetDatabase.Refresh();
            
            FileUtil.DeleteFileOrDirectory("Assets/Settings");
            FileUtil.DeleteFileOrDirectory("Assets/Settings" + ".meta");
            
            AssetDatabase.Refresh();
        }
        

        private static void SetupScene()
        {
            AssetDatabase.RenameAsset("Assets/Scenes/SampleScene.unity", "Main.unity");
            
            AssetDatabase.Refresh();
            
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            GameObject[] allObjects = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Select(t => t.gameObject).ToArray();
            foreach (GameObject go in allObjects)
            {
                go.name = go.name.Replace(' ', '_');
            }
        }


        [MenuItem("Tools/Setup/Import Essential Assets")]
        public static void ImportEssentialAssets()
        {
            Assets.Import("Asset Usage Detector.unitypackage", "yasirkula/Editor ExtensionsUtilities");
            Assets.Import("Graphy - Ultimate FPS Counter - Stats Monitor Debugger.unitypackage", "Tayx/ScriptingGUI");
            Assets.Import("Mulligan Renamer.unitypackage", "Red Blue Games/Editor ExtensionsUtilities");
            Assets.Import("NaughtyAttributes.unitypackage", "Denis Rizov/Editor ExtensionsUtilities");
            Assets.Import("Serialized Dictionary.unitypackage", "ayellowpaper/Editor ExtensionsUtilities");
            Assets.Import("Serialize Interfaces.unitypackage", "ayellowpaper/Editor ExtensionsUtilities");
            Assets.Import("Extenject Dependency Injection IOC.unitypackage", "Mathijs Bakker/Editor ExtensionsUtilities");
            Assets.Import("Rainbow Folders 2.unitypackage", "Borodar/Editor ExtensionsUtilities");
        }

        
        [MenuItem("Tools/Setup/Import Packages")]
        public static void ImportPackages()
        {
            TMP_PackageResourceImporter.ImportResources(true, false, false);
            
            string[] packages =
            {
                "com.unity.recorder",
            };

            Packages.ImportAsync(packages);
        }


        [MenuItem("Tools/Setup/Project Settings")]
        public static void SetProjectSettings()
        {
            const string companyName = "xyperine";
            const string initialVersion = "v0.1.0";

            string projectName = Application.dataPath.Split('/')[^2];
            string defaultNamespace = Regex.Replace(projectName, "\\W|_", "");
            string productName = projectName;
            //Debug.Log(defaultNamespace);
            
            EditorSettings.projectGenerationRootNamespace = defaultNamespace;
            EditorSettings.gameObjectNamingScheme = EditorSettings.NamingScheme.Underscore;
            
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = productName;
            PlayerSettings.bundleVersion = initialVersion;
            
            if (EditorUserBuildSettings.activeBuildTarget is BuildTarget.StandaloneWindows64
                or BuildTarget.StandaloneWindows or BuildTarget.StandaloneLinux64 or BuildTarget.StandaloneOSX)
            {
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);
                Debug.Log("Successfully changed scripting backend to IL2CPP!");
            }
        }


        [MenuItem("Tools/Setup/Project Settings")]
        public static void SetProjectSettings()
        {
            const string companyName = "xyperine";
            const string initialVersion = "v0.1.0";

            string projectName = Application.dataPath.Split('/')[^2];
            string defaultNamespace = Regex.Replace(projectName, "\\W|_", "");
            string productName = projectName;
            //Debug.Log(defaultNamespace);
            
            EditorSettings.projectGenerationRootNamespace = defaultNamespace;
            EditorSettings.gameObjectNamingScheme = EditorSettings.NamingScheme.Underscore;
            
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = productName;
            PlayerSettings.bundleVersion = initialVersion;
            
            if (EditorUserBuildSettings.activeBuildTarget is BuildTarget.StandaloneWindows64
                or BuildTarget.StandaloneWindows or BuildTarget.StandaloneLinux64 or BuildTarget.StandaloneOSX)
            {
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);
                Debug.Log("Successfully changed scripting backend to IL2CPP!");
            }
        }
        
        
        static class Packages
        {
            private static AddRequest _request;
            private static readonly Queue<string> PackagesToInstall = new Queue<string>();
            
            
            public static async Task ImportAsync(string[] packages)
            {
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
        
        
        // TODO: Add support for interactive import
        // TODO: Automatically move imported assets to the plugins folder
        static class Assets
        {
            private const string UNITY_PACKAGE_FILE_EXTENSION = ".unitypackage";
            
            
            public static void Import(string assetName, string folder)
            {
                string basePath;
                if (Environment.OSVersion.Platform is PlatformID.MacOSX or PlatformID.Unix)
                {
                    string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    basePath = Path.Combine(homeDirectory, "Library/Unity/Asset Store-5.x");
                }
                else
                {
                    string defaultPath =
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unity");
                    basePath = Path.Combine(EditorPrefs.GetString("AssetStoreCacheRootPath", defaultPath),
                        "Asset Store-5.x");
                }

                assetName = assetName.EndsWith(UNITY_PACKAGE_FILE_EXTENSION)
                    ? assetName
                    : assetName + UNITY_PACKAGE_FILE_EXTENSION;

                string fullPath = Path.Combine(basePath, folder, assetName);

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"The asset package was not found at the path: {fullPath}");
                }
                
                AssetDatabase.ImportPackage(fullPath, false);
            }
        }
        
        
        // TODO: Create assembly definition files
        static class Folders
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
}
