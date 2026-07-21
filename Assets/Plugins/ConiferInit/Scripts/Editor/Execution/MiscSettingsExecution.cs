using ConiferInit.Editor.Configuration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ConiferInit.Editor.Execution
{
    internal static class MiscSettingsExecution
    {
        public static void Execute(MiscSettings settings)
        {
            if (settings.DeleteTutorial)
            {
                DeleteTutorialAssets();
            }

            if (settings.ConfigureScene)
            {
                SetupScene(settings.SceneName);
            }
        }
        
        
        public static void SetupScene(string sceneName, string originalSceneName = "SampleScene")
        {
            if (!sceneName.EndsWith(".unity"))
            {
                sceneName += ".unity";
            }

            if (!originalSceneName.EndsWith(".unity"))
            {
                originalSceneName += ".unity";
            }
            
            string sampleScenePath = $"Assets/Scenes/{originalSceneName}";
            if (AssetDatabase.AssetPathExists(sampleScenePath))
            {
                AssetDatabase.RenameAsset(sampleScenePath, sceneName);
                
                AssetDatabase.Refresh();
                
                EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}");
            }
            else
            {
                Debug.LogWarning(
                    $"The default scene was not renamed. Couldn't find {originalSceneName}. Either the scene was already renamed earlier, or the project was created with a different default scene.");
            }
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