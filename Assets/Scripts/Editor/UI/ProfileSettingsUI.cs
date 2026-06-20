using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProjectSetup.Editor.UI
{
    internal sealed class ProfileSettingsUI
    {
        private readonly SetupConfiguration _configuration;
        

        public ProfileSettingsUI(SetupConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void Draw()
        {
            GUILayout.Label("Profile", new GUIStyle(EditorStyles.boldLabel));

            using (EditorGUI.ChangeCheckScope changeScope = new EditorGUI.ChangeCheckScope())
            {
                string[] profileNames = _configuration.Profiles.Select(p => p.Name).ToArray();
                int selectedIndex = Array.IndexOf(profileNames, _configuration.ActiveProfile.Name);
                selectedIndex = EditorGUILayout.Popup("Active Profile", selectedIndex, profileNames,
                    new GUIStyle(EditorStyles.popup),GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT));

                if (changeScope.changed)
                {
                    SettingsProfile profile = _configuration.Profiles.Single(p => p.Name == profileNames[selectedIndex]);
                    ApplyProfile(profile);
                }
            }
            
            using (new GUILayout.HorizontalScope(new GUIStyle()))
            {
                using (EditorGUI.DisabledGroupScope s = new EditorGUI.DisabledGroupScope(_configuration.ActiveProfile.Name == _configuration.DefaultProfile.Name))
                {
                    if (GUILayout.Button("Save"))
                    {
                        ConfirmSaveProfileDialog(_configuration.ActiveProfile);
                    }
                }

                if (GUILayout.Button("Save as..."))
                {
                    ShowSaveProfileFilePanel(path => SaveAsProfile(Path.GetFileNameWithoutExtension(path)));
                }
                
                if (GUILayout.Button("New"))
                {
                    ShowSaveProfileFilePanel(path => CreateNewProfile(Path.GetFileNameWithoutExtension(path)));
                }
                
                using (EditorGUI.DisabledGroupScope s = new EditorGUI.DisabledGroupScope(_configuration.ActiveProfile.Name == _configuration.DefaultProfile.Name))
                {
                    if (GUILayout.Button("Delete"))
                    {
                        ConfirmDeleteProfileDialog(_configuration.ActiveProfile);
                    }
                }

                if (GUILayout.Button("Restore"))
                {
                    ApplyProfile(_configuration.ActiveProfile);
                }
            }
        }


        private void ApplyProfile(SettingsProfile profile)
        {
            _configuration.ApplyProfile(profile);
        }


        private void ConfirmDeleteProfileDialog(SettingsProfile profile)
        {
            Assert.IsFalse(profile.Name == SetupConfiguration.DEFAULT_PROFILE_NAME);
            
            if (EditorDialog.DisplayDecisionDialog("Delete Profile?",
                    $"{profile.Name} profile will be irreversibly deleted. Proceed?",
                    "Yes", "No"))
            {
                _configuration.DeleteProfile(profile);
            }
        }


        private void ConfirmSaveProfileDialog(SettingsProfile profile)
        {
            Assert.IsFalse(profile.Name == SetupConfiguration.DEFAULT_PROFILE_NAME);
            
            if (_configuration.Profiles.Exists(p => p.Name == profile.Name))
            {
                if (EditorDialog.DisplayDecisionDialog("Save Profile?",
                        $"This will override the existing {profile.Name} profile. Proceed?",
                        "Yes", "No"))
                {
                    _configuration.SaveProfile(profile);
                }
            }
            else
            {
                _configuration.SaveProfile(profile);
            }
        }


        private void ShowSaveProfileFilePanel(Action<string> onSuccess)
        {
            string newName = _configuration.ConstructNewProfileName();
            
            string savedPath = EditorUtility.SaveFilePanel("New Profile",
                PersistenceSerializer<SettingsProfile>.ProfilesStoragePath, newName, "json");
            if (savedPath != string.Empty)
            {
                _configuration.TrySaveProfileAt(savedPath, onSuccess);
            }
        }


        private void SaveAsProfile(string newProfileName)
        {
            Debug.Log($"Saving as {newProfileName} profile");
            
            if (newProfileName == string.Empty)
            {
                return;
            }
            
            SettingsProfile profile = new SettingsProfile()
            {
                Name = newProfileName,
            };
            
            ConfirmSaveProfileDialog(profile);
        }


        private void CreateNewProfile(string newProfileName)
        {
            ApplyProfile(_configuration.DefaultProfile);
            SaveAsProfile(newProfileName);
        }
    }
}