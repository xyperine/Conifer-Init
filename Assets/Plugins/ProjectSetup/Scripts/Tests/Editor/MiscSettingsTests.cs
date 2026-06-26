using NUnit.Framework;
using ProjectSetupTool.Editor.Execution;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ProjectSetupTool.Editor.Tests
{
    internal sealed class MiscSettingsTests
    {
        [Test]
        public void Scene_is_renamed_from_SampleScene_to_TestScene()
        {
            // Arrange
            string scenePath = "Assets/Scenes/SampleScene.unity";
            if (!AssetDatabase.AssetPathExists(scenePath))
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, scenePath);
            }
            AssetDatabase.Refresh();
            
            // Act
            MiscSettingsExecution.SetupScene("TestScene");
            
            // Assert
            Assert.IsTrue(AssetDatabase.AssetPathExists("Assets/Scenes/TestScene.unity"));
            Assert.IsFalse(AssetDatabase.AssetPathExists("Assets/Scenes/SampleScene.unity"));
        }


        [TearDown]
        public void CleanUp()
        {
            AssetDatabase.DeleteAsset("Assets/Scenes/SampleScene.unity");
            AssetDatabase.DeleteAsset("Assets/Scenes/TestScene.unity");
        }
    }
}