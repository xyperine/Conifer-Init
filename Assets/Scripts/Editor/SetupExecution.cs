using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProjectSetup.Editor
{
    /// <summary>
    /// Handles high-level logic of executing the setup by coordinating other components.
    /// </summary>
    internal static class SetupExecution
    {
        public static void CreateFolders(string[] folders)
        {
            Folders.Create(string.Empty, folders);
            
            AssetDatabase.Refresh();
        }


        public static void ImportAssetsInteractive(IEnumerable<AssetImportEntry> assets)
        {
            Assert.IsTrue(assets.All(a => a.Interactive));
            
            Assets.ImportInteractive(assets);
        }


        public static void ImportAssetsNonInteractive(IEnumerable<AssetImportEntry> assets)
        {
            Assert.IsTrue(assets.All(a => !a.Interactive));
            
            foreach (AssetImportEntry asset in assets)
            {
                Assets.Import(asset.Path, asset.Interactive);
            }
        }


        public static void ImportPackages(IEnumerable<string> packages)
        {
            TMP_PackageResourceImporter.ImportResources(true, false, false);

            if (packages.Any())
            {
                Packages.ImportAsync(packages);
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
        }


        public static void ExecuteMisc(MiscSettings miscSettings)
        {
            if (miscSettings.DeleteTutorial)
            {
                DeleteTutorialAssets();
            }

            if (miscSettings.DeleteTutorial)
            {
                SetupScene(miscSettings.SceneName);
            }
        }


        private static void SetupScene(string sceneName)
        {
            if (!sceneName.EndsWith(".unity"))
            {
                sceneName += ".unity";
            }
            
            AssetDatabase.RenameAsset("Assets/Scenes/SampleScene.unity", sceneName);
            
            AssetDatabase.Refresh();
            
            EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}");
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
    }
}
