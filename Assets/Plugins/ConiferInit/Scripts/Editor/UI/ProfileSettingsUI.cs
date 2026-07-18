using System;
using System.IO;
using System.Linq;
using ConiferInit.Editor.Configuration;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ConiferInit.Editor.UI
{
    internal sealed class ProfileSettingsUI
    {
        private readonly SettingsProfileConfiguration _configuration;
        private readonly Styles _styles;
        

        public ProfileSettingsUI(SettingsProfileConfiguration configuration, Styles styles)
        {
            _configuration = configuration;
            _styles = styles;
        }


        public void Draw()
        {
            GUILayout.Label("Profile", _styles.SectionTitle);

            using (EditorGUI.ChangeCheckScope changeScope = new EditorGUI.ChangeCheckScope())
            {
                string[] profileNames = _configuration.Profiles.Select(p => p.Name).ToArray();
                int selectedIndex = Array.IndexOf(profileNames, _configuration.ActiveProfile.Name);
                selectedIndex = EditorGUILayout.Popup("Active Profile", selectedIndex, profileNames,
                    _styles.Popup,GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT));

                if (changeScope.changed)
                {
                    SettingsProfile profile = _configuration.Profiles.Single(p => p.Name == profileNames[selectedIndex]);
                    ApplyProfile(profile);
                }
            }
            
            using (new GUILayout.HorizontalScope(_styles.Scope))
            {
                if (GUILayout.Button("New"))
                {
                    ShowCreateNewProfileFilePanel();
                }

                using (EditorGUI.DisabledGroupScope s = new EditorGUI.DisabledGroupScope(_configuration.ActiveProfile.Name == _configuration.DefaultProfile.Name))
                {
                    if (GUILayout.Button("Save"))
                    {
                        ConfirmSaveProfileDialog(_configuration.ActiveProfile);
                    }
                }

                if (GUILayout.Button("Save as..."))
                {
                    ShowSaveProfileFilePanel();
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


        private void ConfirmSaveProfileDialog(SettingsProfile profile)
        {
            Assert.IsFalse(profile.Name == SettingsProfileConfiguration.DEFAULT_PROFILE_NAME);
            
            if (_configuration.Profiles.Exists(p => p.Name == profile.Name))
            {
                bool wantToSaveProfile = Dialog.DisplayDecisionDialog("Save Profile?",
                    $"This will override the existing {profile.Name} profile. Proceed?",
                    "Yes", "No");
                if (wantToSaveProfile)
                {
                    _configuration.SaveProfile(profile);
                }
            }
            else
            {
                _configuration.SaveProfile(profile);
            }
        }


        private void ConfirmDeleteProfileDialog(SettingsProfile profile)
        {
            Assert.IsFalse(profile.Name == SettingsProfileConfiguration.DEFAULT_PROFILE_NAME);

            bool wantToDeleteProfile = Dialog.DisplayDecisionDialog("Delete Profile?",
                $"{profile.Name} profile will be irreversibly deleted. Proceed?",
                "Yes", "No");
            if (wantToDeleteProfile)
            {
                _configuration.DeleteProfile(profile);
            }
        }


        private void ShowSaveProfileFilePanel()
        {
            string newName = _configuration.ConstructNewProfileName();
            
            string savedPath = EditorUtility.SaveFilePanel("New Profile",
                SettingsProfilePersistency.StoragePath, newName, "json");
            if (savedPath != string.Empty)
            {
                if (_configuration.IsValidProfilePath(savedPath))
                {
                    TrySaveAsProfile(Path.GetFileNameWithoutExtension(savedPath));
                }
            }
        }


        private void TrySaveAsProfile(string newProfileName)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(newProfileName));
            
            SettingsProfile profile = new SettingsProfile()
            {
                Name = newProfileName,
            };
            
            ConfirmSaveProfileDialog(profile);
        }
        
        
        private void ShowCreateNewProfileFilePanel()
        {
            bool wantToCreateNewProfile = Dialog.DisplayDecisionDialog("New Profile",
                $"Unsaved changes to the {_configuration.ActiveProfile.Name} will be lost.",
                "Continue", "Cancel");
            if (!wantToCreateNewProfile)
            {
                return;
            }
            
            string newName = _configuration.ConstructNewProfileName();
            
            string savedPath = EditorUtility.SaveFilePanel("New Profile",
                SettingsProfilePersistency.StoragePath, newName, "json");
            if (savedPath != string.Empty)
            {
                if (_configuration.IsValidNewProfilePath(savedPath))
                {
                    CreateNewProfile(Path.GetFileNameWithoutExtension(savedPath));
                }
            }
        }
        

        private void CreateNewProfile(string newProfileName)
        {
            Assert.IsFalse(newProfileName == SettingsProfileConfiguration.DEFAULT_PROFILE_NAME);
            Assert.IsFalse(_configuration.Profiles.Exists(p => p.Name == newProfileName));
            Assert.IsFalse(string.IsNullOrWhiteSpace(newProfileName));
            
            ApplyProfile(_configuration.DefaultProfile);
            
            SettingsProfile profile = new SettingsProfile()
            {
                Name = newProfileName,
            };
        
            _configuration.SaveProfile(profile);
        }
    }
}