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
        private const int MAX_ENTRIES_PER_PAGE = 10;
        
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
        
        private int _availablePage = 1;
        private int _queuedPage = 1;

        private string _searchString;


        public PackagesSettingsUI(SetupConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void ResetTemporaryState()
        {
            _availablePage = 1;
            _queuedPage = 1;
            _searchString = string.Empty;
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
                
                _scopeStyle = new GUIStyle();

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
            
            SetupWindowElements.DrawRegularSpace();
            
            List<string> availablePackageIDs = _configuration.AvailablePackages;
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                availablePackageIDs = _configuration.FindPackages(_searchString);

                _availablePage = 1;
            }
            
            // Available list
            List<string> queuedPackageIDs = _configuration.GetQueuedPackageIDs();
            using (new GUILayout.VerticalScope($"Available ({availablePackageIDs.Count})", _windowStyle))
            {
                if (availablePackageIDs.Count > 0)
                {
                    int start = (_availablePage - 1) * MAX_ENTRIES_PER_PAGE;
                    int entriesCount = Math.Min(MAX_ENTRIES_PER_PAGE,
                        availablePackageIDs.Count - (_availablePage - 1) * MAX_ENTRIES_PER_PAGE);
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
                            
                    using (new EditorGUI.DisabledGroupScope(_availablePage <= 1))
                    {
                        if (GUILayout.Button("<"))
                        {
                            _availablePage--;
                        }
                    }
                            
                    int maxPages =
                        Mathf.CeilToInt(availablePackageIDs.Count / (float) MAX_ENTRIES_PER_PAGE);
                    GUILayout.Label($"{_availablePage}/{maxPages}", _labelStyle);
                            
                    using (new EditorGUI.DisabledGroupScope(_availablePage >= maxPages))
                    {
                        if (GUILayout.Button(">"))
                        {
                            _availablePage++;
                        }
                    }
                }
                else
                {
                    SetupWindowElements.DrawEmptyListElement();
                }
            }

            SetupWindowElements.DrawRegularSpace();
                
            // Queued list
            using (new GUILayout.VerticalScope($"Queued ({queuedPackageIDs.Count})", _windowStyle))
            {
                if (queuedPackageIDs.Count > 0)
                {
                    int start = (_queuedPage - 1) * MAX_ENTRIES_PER_PAGE;
                    int entriesCount = Math.Min(MAX_ENTRIES_PER_PAGE,
                        queuedPackageIDs.Count - (_queuedPage - 1) * MAX_ENTRIES_PER_PAGE);
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
                    if (queuedPackageIDs.Count > MAX_ENTRIES_PER_PAGE)
                    {
                        using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(_scopeStyle);
                        GUILayout.FlexibleSpace();

                        using (new EditorGUI.DisabledGroupScope(_queuedPage <= 1))
                        {
                            if (GUILayout.Button("<"))
                            {
                                _queuedPage--;
                            }
                        }

                        int maxPages =
                            Mathf.CeilToInt(queuedPackageIDs.Count / (float) MAX_ENTRIES_PER_PAGE);
                        GUILayout.Label($"{_queuedPage}/{maxPages}", _labelStyle);

                        using (new EditorGUI.DisabledGroupScope(_queuedPage >= maxPages))
                        {
                            if (GUILayout.Button(">"))
                            {
                                _queuedPage++;
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