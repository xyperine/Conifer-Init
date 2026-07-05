using System.Collections.Generic;
using ConiferInit.Editor.Configuration;
using ConiferInit.Editor.Execution;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;

namespace ConiferInit.Editor.Tests
{
    internal sealed class ProjectSettingsTests
    {
        private ProjectSettings _temp;

        [SetUp]
        public void SetUp()
        {
            _temp = GetCurrentSettings();
        }
        
        
        [Test]
        public void Execution_changes_project_settings_correctly()
        {
            // Arrange
            ProjectSettings projectSettings = CreateTestProjectSettings();

            // Act
            ProjectSettingsExecution.Set(projectSettings);

            // Assert
            Assert.AreEqual("test", EditorSettings.projectGenerationRootNamespace);
            Assert.AreEqual(EditorSettings.NamingScheme.SpaceParenthesis, EditorSettings.gameObjectNamingScheme);
            Assert.AreEqual("testcompany", PlayerSettings.companyName);
            Assert.AreEqual("testproduct", PlayerSettings.productName);
            Assert.AreEqual("testversion", PlayerSettings.bundleVersion);
            
            // Not sure about scripting backends testing, not all targets can have any of the 3 options, so if we set
            // test scripting implementation to anything other than IL2CPP it will fail.
            foreach (NamedBuildTarget target in NamedBuildTargetHelpers.AllTargets)
            {
                Assert.AreEqual(ScriptingImplementation.IL2CPP, PlayerSettings.GetScriptingBackend(target));
            }
        }


        private ProjectSettings CreateTestProjectSettings()
        {
            return new ProjectSettings("test", EditorSettings.NamingScheme.SpaceParenthesis, "testcompany",
                "testproduct", "testversion", ScriptingImplementation.IL2CPP);
        }


        private ProjectSettings GetCurrentSettings()
        {
            Dictionary<string, ScriptingImplementation> backends =
                new Dictionary<string, ScriptingImplementation>();

            foreach (NamedBuildTarget target in NamedBuildTargetHelpers.AllTargets)
            {
                backends.Add(target.TargetName, PlayerSettings.GetScriptingBackend(target));
            }
            
            return new ProjectSettings(EditorSettings.projectGenerationRootNamespace,
                EditorSettings.gameObjectNamingScheme, PlayerSettings.companyName, PlayerSettings.productName,
                PlayerSettings.bundleVersion, backends);
        }


        [TearDown]
        public void CleanUp()
        {
            EditorSettings.projectGenerationRootNamespace = _temp.DefaultNamespace;
            EditorSettings.gameObjectNamingScheme = _temp.GameObjectNamingScheme;
            
            PlayerSettings.companyName = _temp.CompanyName;
            PlayerSettings.productName = _temp.ProductName;
            PlayerSettings.bundleVersion = _temp.Version;

            foreach (ProjectSettings.ScriptingBackendEntry backend in _temp.Backends)
            {
                NamedBuildTarget target = NamedBuildTargetHelpers.FindByName(backend.TargetName);
                PlayerSettings.SetScriptingBackend(target, backend.Implementation);
            }
        }
    }
}