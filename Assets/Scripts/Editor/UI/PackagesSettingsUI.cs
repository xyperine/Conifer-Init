using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ProjectSetup.Editor.UI
{
    public class PackagesSettingsUI
    {
        private readonly SetupConfiguration _configuration;

        private GUIStyle _titleStyle;
        private GUIStyle _searchBarStyle;
        private GUIStyle _windowStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _successStyle;
        private GUIStyle _errorStyle;
        private GUIStyle _scopeStyle;

        private bool _stylesInitialized;
        
        private int _availablePackagesPage = 1;
        private int _queuedPackagesPage = 1;

        private string _packagesSearchString;


        public PackagesSettingsUI(SetupConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void ResetTemporaryState()
        {
            _packagesSearchString = string.Empty;
        }
        
        
        public void Draw()
        {
            if (!_stylesInitialized)
            {
                _titleStyle = new GUIStyle(EditorStyles.boldLabel);
                _searchBarStyle = new GUIStyle(EditorStyles.toolbarSearchField);
                _windowStyle = new GUIStyle(GUI.skin.window);
                _labelStyle = new GUIStyle(GUI.skin.label);
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _successStyle = new GUIStyle(GUI.skin.label)
                    {normal = new GUIStyleState() {textColor = Color.limeGreen}};
                _errorStyle = new GUIStyle(GUI.skin.label)
                    {normal = new GUIStyleState() {textColor = Color.crimson}};
                _scopeStyle = new GUIStyle();

                _stylesInitialized = true;
            }
            
            const int maxEntriesPerPage = 10;
            
            GUILayout.Label("Packages Settings", _titleStyle);

            if (!SuccessfullyRetrievedPackages())
            {
                return;
            }

            // Search feature
            _packagesSearchString =
                GUILayout.TextField(_packagesSearchString, _searchBarStyle, GUILayout.MaxWidth(256f));
            
            List<string> availablePackageIDs = _configuration.AvailablePackages;
            if (!string.IsNullOrWhiteSpace(_packagesSearchString))
            {
                availablePackageIDs = _configuration.FindPackages(_packagesSearchString);

                _availablePackagesPage = 1;
            }
            
            // Available list
            List<string> queuedPackageIDs = _configuration.GetQueuedPackageIDs();
            using (new GUILayout.VerticalScope($"Available ({availablePackageIDs.Count})", _windowStyle))
            {
                if (availablePackageIDs.Count > 0)
                {
                    int start = (_availablePackagesPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        availablePackageIDs.Count - (_availablePackagesPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        PackageInfo packageInfo = _configuration.GetPackageByID(availablePackageIDs[i]);
                        using EditorGUILayout.HorizontalScope entryScope = new EditorGUILayout.HorizontalScope(_scopeStyle);
                        
                        SetupWindowElements.DrawListElementBackground(entryScope.rect, i);
                                
                        GUILayout.Label(packageInfo.displayName, _labelStyle, GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT), GUILayout.MinWidth(128f));
                        if (GUILayout.Button("Import", _buttonStyle, GUILayout.Width(64f), GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT)))
                        {
                            _configuration.QueuePackage(availablePackageIDs[i]);

                            i--;
                        }
                    }

                    // Pages navigation
                    using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(_scopeStyle);
                    GUILayout.FlexibleSpace();
                            
                    using (new EditorGUI.DisabledGroupScope(_availablePackagesPage <= 1))
                    {
                        if (GUILayout.Button("<"))
                        {
                            _availablePackagesPage--;
                        }
                    }
                            
                    int maxPages =
                        Mathf.CeilToInt(availablePackageIDs.Count / (float) maxEntriesPerPage);
                    GUILayout.Label($"{_availablePackagesPage}/{maxPages}", _labelStyle);
                            
                    using (new EditorGUI.DisabledGroupScope(_availablePackagesPage >= maxPages))
                    {
                        if (GUILayout.Button(">"))
                        {
                            _availablePackagesPage++;
                        }
                    }
                }
                else
                {
                    SetupWindowElements.DrawEmptyListElement();
                }
            }

            SetupWindowElements.DrawSmallSpace();
                
            // Queued list
            using (new GUILayout.VerticalScope($"Queued ({queuedPackageIDs.Count})", _windowStyle))
            {
                if (queuedPackageIDs.Count > 0)
                {
                    int start = (_queuedPackagesPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        queuedPackageIDs.Count - (_queuedPackagesPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        PackageInfo packageInfo = _configuration.GetPackageByID(queuedPackageIDs[i]);
                        using EditorGUILayout.HorizontalScope entryScope =
                            new EditorGUILayout.HorizontalScope(_scopeStyle);
                        
                        SetupWindowElements.DrawListElementBackground(entryScope.rect, i);

                        GUILayout.Label(packageInfo.displayName, _labelStyle, GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT),
                            GUILayout.MinWidth(128f));

                        if (GUILayout.Button("Remove", _buttonStyle, GUILayout.Width(64f),
                                GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT)))
                        {
                            _configuration.DequeuePackage(queuedPackageIDs[i]);

                            i--;
                            entriesCount--;
                        }
                    }

                    // Pages navigation
                    if (queuedPackageIDs.Count > maxEntriesPerPage)
                    {
                        using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(_scopeStyle);
                        GUILayout.FlexibleSpace();

                        using (new EditorGUI.DisabledGroupScope(_queuedPackagesPage <= 1))
                        {
                            if (GUILayout.Button("<"))
                            {
                                _queuedPackagesPage--;
                            }
                        }

                        int maxPages =
                            Mathf.CeilToInt(queuedPackageIDs.Count / (float) maxEntriesPerPage);
                        GUILayout.Label($"{_queuedPackagesPage}/{maxPages}", _labelStyle);

                        using (new EditorGUI.DisabledGroupScope(_queuedPackagesPage >= maxPages))
                        {
                            if (GUILayout.Button(">"))
                            {
                                _queuedPackagesPage++;
                            }
                        }
                    }
                }
                else
                {
                    SetupWindowElements.DrawEmptyListElement();
                }
            }
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
                    //Debug.Log("Successfully retrieved packages.");
                    GUILayout.Label($"Retrieved packages: {_configuration.PackagesListRequest.Result.Length}", _successStyle);
                    break;
                case StatusCode.Failure:
                    //Debug.Log("Failed to retrieve packages.");
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