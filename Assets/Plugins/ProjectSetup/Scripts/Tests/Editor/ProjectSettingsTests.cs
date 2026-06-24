using System.Collections.Generic;
using NUnit.Framework;
using ProjectSetupTool.Editor.Configuration;
using ProjectSetupTool.Editor.Execution;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ProjectSetupTool.Editor.Tests
{
    public class ProjectSettingsTests
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
            foreach (NamedBuildTarget target in ProjectSettings.AllTargets)
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
            Dictionary<NamedBuildTarget, ScriptingImplementation> backends =
                new Dictionary<NamedBuildTarget, ScriptingImplementation>();

            foreach (NamedBuildTarget target in ProjectSettings.AllTargets)
            {
                backends.Add(target, PlayerSettings.GetScriptingBackend(target));
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
                PlayerSettings.SetScriptingBackend(backend.Target, backend.Implementation);
            }
        }
    }
}