using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProjectSetup.Editor
{
    public class ProfileSettingsUI
    {
        private readonly SetupBusiness _business;
        

        public ProfileSettingsUI(SetupBusiness business)
        {
            _business = business;
        }


        public void Draw()
        {
            GUILayout.Label("Profile", new GUIStyle(EditorStyles.boldLabel));

            using (EditorGUI.ChangeCheckScope changeScope = new EditorGUI.ChangeCheckScope())
            {
                string[] profileNames = _business.Profiles.Select(p => p.Name).ToArray();
                int selectedIndex = Array.IndexOf(profileNames, _business.ActiveProfile.Name);
                selectedIndex = EditorGUILayout.Popup("Active Profile", selectedIndex, profileNames,
                    new GUIStyle(EditorStyles.popup),GUILayout.Height(16f));

                if (changeScope.changed)
                {
                    SettingsProfile profile = _business.Profiles.Single(p => p.Name == profileNames[selectedIndex]);
                    ApplyProfile(profile);
                }
            }
            
            using (new GUILayout.HorizontalScope(new GUIStyle()))
            {
                using (EditorGUI.DisabledGroupScope s = new EditorGUI.DisabledGroupScope(_business.ActiveProfile.Name == _business.DefaultProfile.Name))
                {
                    if (GUILayout.Button("Save"))
                    {
                        ConfirmSaveProfileDialog(_business.ActiveProfile);
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
                
                using (EditorGUI.DisabledGroupScope s = new EditorGUI.DisabledGroupScope(_business.ActiveProfile.Name == _business.DefaultProfile.Name))
                {
                    if (GUILayout.Button("Delete"))
                    {
                        ConfirmDeleteProfileDialog(_business.ActiveProfile);
                    }
                }

                if (GUILayout.Button("Restore"))
                {
                    ApplyProfile(_business.ActiveProfile);
                }
            }
        }


        private void ApplyProfile(SettingsProfile profile)
        {
            _business.ApplyProfile(profile);
        }


        private void ConfirmDeleteProfileDialog(SettingsProfile profile)
        {
            Assert.IsFalse(profile.Name == SetupBusiness.DEFAULT_PROFILE_NAME);
            
            if (EditorDialog.DisplayDecisionDialog("Delete Profile?",
                    $"{profile.Name} profile will be irreversibly deleted. Proceed?",
                    "Yes", "No"))
            {
                _business.DeleteProfile(profile);
            }
        }


        private void ConfirmSaveProfileDialog(SettingsProfile profile)
        {
            Assert.IsFalse(profile.Name == SetupBusiness.DEFAULT_PROFILE_NAME);
            
            if (_business.Profiles.Exists(p => p.Name == profile.Name))
            {
                if (EditorDialog.DisplayDecisionDialog("Save Profile?",
                        $"This will override the existing {profile.Name} profile. Proceed?",
                        "Yes", "No"))
                {
                    _business.SaveProfile(profile);
                }
            }
            else
            {
                _business.SaveProfile(profile);
            }
        }


        private void ShowSaveProfileFilePanel(Action<string> onSuccess)
        {
            string newName = _business.ConstructNewProfileName();
            
            string savedPath = EditorUtility.SaveFilePanel("New Profile",
                PersistenceSerializer<SettingsProfile>.ProfilesStoragePath, newName, "json");
            if (savedPath != string.Empty)
            {
                _business.TrySaveProfileAt(savedPath, onSuccess);
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
            ApplyProfile(_business.DefaultProfile);
            SaveAsProfile(newProfileName);
        }
    }
}