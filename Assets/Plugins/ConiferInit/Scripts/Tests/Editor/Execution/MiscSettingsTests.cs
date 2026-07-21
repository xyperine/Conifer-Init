using ConiferInit.Editor.Execution;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ConiferInit.Editor.Tests.Execution
{
    internal sealed class MiscSettingsTests
    {
        private const string ORIGINAL_SCENE_NAME = "TestSampleScene";
        private const string NEW_SCENE_NAME = "RenamedTestScene";
        
            
        [Test]
        public void Scene_is_renamed_from_SampleScene_to_TestScene()
        {
            // Arrange
            string scenePath = $"Assets/Scenes/{ORIGINAL_SCENE_NAME}.unity";
            if (!AssetDatabase.AssetPathExists(scenePath))
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, scenePath);
            }
            AssetDatabase.Refresh();

            
            // Act
            MiscSettingsExecution.SetupScene(NEW_SCENE_NAME, ORIGINAL_SCENE_NAME);
            
            // Assert
            Assert.IsTrue(AssetDatabase.AssetPathExists($"Assets/Scenes/{NEW_SCENE_NAME}.unity"));
            Assert.IsFalse(AssetDatabase.AssetPathExists($"Assets/Scenes/{ORIGINAL_SCENE_NAME}.unity"));
        }


        [TearDown]
        public void CleanUp()
        {
            AssetDatabase.DeleteAsset($"Assets/Scenes/{ORIGINAL_SCENE_NAME}.unity");
            AssetDatabase.DeleteAsset($"Assets/Scenes/{NEW_SCENE_NAME}.unity");
        }
    }
}