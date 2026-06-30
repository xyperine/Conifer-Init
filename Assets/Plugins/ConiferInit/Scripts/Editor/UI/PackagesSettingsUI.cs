using System;
using System.Collections.Generic;
using ConiferInit.Editor.Configuration;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ConiferInit.Editor.UI
{
    internal sealed class PackagesSettingsUI
    {
        private readonly SetupConfiguration _configuration;

        private readonly ListDrawer _availableListDrawer;
        private readonly ListDrawer _queuedListDrawer;

        private GUIStyle _titleStyle;
        private GUIStyle _searchBarStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _successStyle;
        private GUIStyle _errorStyle;

        private bool _stylesInitialized;

        private string _searchString;


        public PackagesSettingsUI(SetupConfiguration configuration)
        {
            _configuration = configuration;

            _availableListDrawer = new ListDrawer();
            _queuedListDrawer = new ListDrawer();
        }


        public void ResetTemporaryState()
        {
            _availableListDrawer.Reset();
            _queuedListDrawer.Reset();
            
            _searchString = string.Empty;
        }
        
        
        public void Draw()
        {
            if (!_stylesInitialized)
            {
                _titleStyle = new GUIStyle(EditorStyles.boldLabel);
                _searchBarStyle = new GUIStyle(EditorStyles.toolbarSearchField);
                _labelStyle = new GUIStyle(GUI.skin.label);
                _buttonStyle = new GUIStyle(GUI.skin.button);
                
                _successStyle = new GUIStyle(GUI.skin.label);
                _successStyle.normal = new GUIStyleState() {textColor = Color.limeGreen};
                _successStyle.hover = new GUIStyleState() {textColor = Color.limeGreen};
                _successStyle.active = new GUIStyleState() {textColor = Color.limeGreen};
                _successStyle.focused = new GUIStyleState() {textColor = Color.limeGreen};
                _successStyle.wordWrap = true;
                
                _errorStyle = new GUIStyle(GUI.skin.label);
                _errorStyle.normal = new GUIStyleState() {textColor = Color.crimson};
                _errorStyle.hover = new GUIStyleState() {textColor = Color.crimson};
                _errorStyle.active = new GUIStyleState() {textColor = Color.crimson};
                _errorStyle.focused = new GUIStyleState() {textColor = Color.crimson};
                _errorStyle.wordWrap = true;
                
                _stylesInitialized = true;
            }

            GUILayout.Label("Packages Settings", _titleStyle);

            if (!SuccessfullyRetrievedPackages())
            {
                return;
            }

            // Search feature
            _searchString =
                GUILayout.TextField(_searchString, _searchBarStyle, GUILayout.MaxWidth(256f));
            
            WindowElements.DrawRegularSpace();
            
            List<string> availablePackageIDs = _configuration.AvailablePackages;
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                availablePackageIDs = _configuration.FindPackages(_searchString);

                _availableListDrawer.Reset();
            }
            
            // Available list
            _availableListDrawer.Draw(availablePackageIDs, id =>
            {
                PackageInfo packageInfo = _configuration.GetPackageByID(id);
                                
                GUILayout.Label(packageInfo.displayName, _labelStyle, GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT), GUILayout.MinWidth(128f));
                if (GUILayout.Button("Import", _buttonStyle, GUILayout.Width(64f), GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT)))
                {
                    _configuration.QueuePackage(id);
                }
            }, $"Available ({availablePackageIDs.Count})");

            WindowElements.DrawRegularSpace();

            // Queued list
            List<PackageImportEntry> queuedPackages = _configuration.GetQueuedPackageIDs();
            _queuedListDrawer.Draw(queuedPackages, entry =>
            {
                PackageInfo packageInfo = _configuration.GetPackageByID(entry.ShortID);

                GUILayout.Label(packageInfo.displayName, _labelStyle, GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT),
                    GUILayout.MinWidth(128f));

                if (GUILayout.Button("Remove", _buttonStyle, GUILayout.Width(64f),
                        GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT)))
                {
                    _configuration.DequeuePackage(entry.ShortID);
                }
            }, $"Queued ({queuedPackages.Count})");
        }


        // This routine is poorly designed so I don't know what to do with it 
        private bool SuccessfullyRetrievedPackages()
        {
            switch (_configuration.PackagesListRequest.Status)
            {
                case StatusCode.InProgress:
                    GUILayout.Label("Retrieving packages...");
                    break;
                case StatusCode.Success:
                    GUILayout.Label($"Retrieved packages: {_configuration.PackagesListRequest.Result.Length}", _successStyle);
                    break;
                case StatusCode.Failure:
                    GUILayout.Label($"Error while retrieving packages: {_configuration.PackagesListRequest.Error.message}", _errorStyle);
                    break;
                default:
                    Debug.LogError("Invalid request");
                    throw new ArgumentOutOfRangeException();
            }
            
            return _configuration.SuccessfullyRetrievedPackages();
        }
    }
}