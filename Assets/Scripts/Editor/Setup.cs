using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using Application = UnityEngine.Application;
using Object = UnityEngine.Object;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Handles high-level logic coordinating other components.
    /// </summary>
    public static class Setup
    {
        [MenuItem("Tools/Setup/Folder Structure")]
        public static void FolderStructure()
        {
            CreateFolders();
            
            DeleteTutorialAssets();

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
        

        private static void SetupScene()
        {
            AssetDatabase.RenameAsset("Assets/Scenes/SampleScene.unity", "Main.unity");
            
            AssetDatabase.Refresh();
            
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        }


        [MenuItem("Tools/Setup/Import Essential Assets")]
        public static void ImportEssentialAssets()
        {
            Assets.Import("Asset Usage Detector.unitypackage", "yasirkula/Editor ExtensionsUtilities", false);
            Assets.Import("Graphy - Ultimate FPS Counter - Stats Monitor Debugger.unitypackage", "Tayx/ScriptingGUI", false);
            Assets.Import("Mulligan Renamer.unitypackage", "Red Blue Games/Editor ExtensionsUtilities", false);
            Assets.Import("NaughtyAttributes.unitypackage", "Denis Rizov/Editor ExtensionsUtilities", false);
            Assets.Import("Serialized Dictionary.unitypackage", "ayellowpaper/Editor ExtensionsUtilities", false);
            Assets.Import("Serialize Interfaces.unitypackage", "ayellowpaper/Editor ExtensionsUtilities", false);
            Assets.Import("Extenject Dependency Injection IOC.unitypackage", "Mathijs Bakker/Editor ExtensionsUtilities", false);
            Assets.Import("Rainbow Folders 2.unitypackage", "Borodar/Editor ExtensionsUtilities", false);
        }


        // First, sort the array by interactiveness
        // Then, perform non-interactive imports
        // Then, perform interactive imports. Chain them, waiting for the completion of each one.
        public static void ImportAssets(IEnumerable<AssetInfo> assets)
        {
            var nonInteractive = assets.Where(a => !a.Interactive);
            var interactive = assets.Where(a => a.Interactive);
            
            foreach (AssetInfo assetInfo in nonInteractive)
            {
                Assets.Import(assetInfo.Path, assetInfo.Interactive);
            }

            if (interactive.Any())
            {
                Assets.ImportInteractive(interactive);
            }
        }

        
        [MenuItem("Tools/Setup/Import Packages")]
        public static void ImportPackages()
        {
            if (PackageInfo.FindForAssetPath("Packages/com.unity.textmeshpro") == null)
            {
                TMP_PackageResourceImporter.ImportResources(true, false, false);
            }
            
            string[] packages =
            {
                "com.unity.recorder",
            };

            Packages.ImportAsync(packages);
        }
        
        
        public static void ImportPackages(IEnumerable<string> packages)
        {
            TMP_PackageResourceImporter.ImportResources(true, false, false);

            if (packages.Any())
            {
                Packages.ImportAsync(packages);
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


        public static void SetProjectSettings(ProjectSettings projectSettings)
        {
            EditorSettings.projectGenerationRootNamespace = projectSettings.DefaultNamespace;
            EditorSettings.gameObjectNamingScheme = projectSettings.GameObjectNamingScheme;
            
            PlayerSettings.companyName = projectSettings.CompanyName;
            PlayerSettings.productName = projectSettings.ProductName;
            PlayerSettings.bundleVersion = projectSettings.Version;

            NamedBuildTarget[] buildTargets =
            {
                NamedBuildTarget.Android,
                NamedBuildTarget.EmbeddedLinux,
                NamedBuildTarget.iOS,
                NamedBuildTarget.LinuxHeadlessSimulation,
                NamedBuildTarget.NintendoSwitch,
                NamedBuildTarget.NintendoSwitch2,
                NamedBuildTarget.PS4,
                NamedBuildTarget.PS5,
                NamedBuildTarget.QNX,
                NamedBuildTarget.Server,
                NamedBuildTarget.Standalone,
                NamedBuildTarget.tvOS,
                NamedBuildTarget.VisionOS,
                NamedBuildTarget.WebGL,
                NamedBuildTarget.WindowsStoreApps,
                NamedBuildTarget.XboxOne,
            };

            foreach (NamedBuildTarget buildTarget in buildTargets)
            {
                PlayerSettings.SetScriptingBackend(buildTarget, projectSettings.ScriptingBackend);
            }
            
            Debug.Log("Project settings set");
        }


        // TODO: Use sceneName
        public static void ExecuteMisc(bool deleteTutorial, bool configureScene, string sceneName)
        {
            if (deleteTutorial)
            {
                DeleteTutorialAssets();
            }

            if (configureScene)
            {
                SetupScene();
            }
        }


        public static void ExecuteMisc(MiscSettings miscSettings)
        {
            ExecuteMisc(miscSettings.DeleteTutorial, miscSettings.ConfigureScene, miscSettings.SceneName);
        }
    }
}
