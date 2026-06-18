using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor.UI
{
    public class AssetsSettingsUI
    {
        private const int MAX_ENTRIES_PER_PAGE = 10;
        
        private readonly SetupConfiguration _configuration;

        private GUIStyle _titleStyle;
        private GUIStyle _searchBarStyle;
        private GUIStyle _windowStyle;
        private GUIStyle _scopeStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;

        private bool _stylesInitialized;

        private int _availablePage = 1;
        private int _queuedPage = 1;

        private string _searchString;
        

        public AssetsSettingsUI(SetupConfiguration configuration)
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
                _scopeStyle = new GUIStyle();
                _labelStyle = new GUIStyle(GUI.skin.label);
                _buttonStyle = new GUIStyle(GUI.skin.button);

                _stylesInitialized = true;
            }

            GUILayout.Label("Assets Settings", _titleStyle);

            if (!_configuration.SuccessfullyRetrievedAssets)
            {
                _configuration.RetrieveCachedAssets();
                
                return;
            }
            
            // Search feature
            _searchString =
                GUILayout.TextField(_searchString, _searchBarStyle,
                    GUILayout.MaxWidth(256f));
            
            List<string> availableAssetIDs = _configuration.AvailableAssets;
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                availableAssetIDs = _configuration.FindAssets(_searchString);

                _availablePage = 1;
            }
            
            // Available list
            List<AssetImportEntry> queuedAssets = _configuration.GetQueuedAssets();
            using (new GUILayout.VerticalScope($"Available ({availableAssetIDs.Count})", _windowStyle))
            {
                if (availableAssetIDs.Count > 0)
                {
                    int start = (_availablePage - 1) * MAX_ENTRIES_PER_PAGE;
                    int entriesCount = Math.Min(MAX_ENTRIES_PER_PAGE,
                        availableAssetIDs.Count - (_availablePage - 1) * MAX_ENTRIES_PER_PAGE);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        string assetName = _configuration.FindAssetByID(availableAssetIDs[i]).Name;
                        using EditorGUILayout.HorizontalScope entryScope = new EditorGUILayout.HorizontalScope(_scopeStyle);
                        
                        SetupWindowElements.DrawListElementBackground(entryScope.rect, i);
                                
                        GUILayout.Label(assetName, _labelStyle, GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT), GUILayout.MinWidth(128f));
                        if (GUILayout.Button("Import", _buttonStyle, GUILayout.Width(64f), GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT)))
                        {
                            _configuration.QueueAsset(availableAssetIDs[i]);

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
                        Mathf.CeilToInt(availableAssetIDs.Count / (float) MAX_ENTRIES_PER_PAGE);
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

            SetupWindowElements.DrawSmallSpace();
                
            // Queued list
            using (new GUILayout.VerticalScope($"Queued ({queuedAssets.Count})", _windowStyle))
            {
                if (queuedAssets.Count > 0)
                {
                    int start = (_queuedPage - 1) * MAX_ENTRIES_PER_PAGE;
                    int entriesCount = Math.Min(MAX_ENTRIES_PER_PAGE,
                        queuedAssets.Count - (_queuedPage - 1) * MAX_ENTRIES_PER_PAGE);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        AssetImportEntry asset = queuedAssets[i];
                        using EditorGUILayout.HorizontalScope entryScope =
                            new EditorGUILayout.HorizontalScope(_scopeStyle);
                        
                        SetupWindowElements.DrawListElementBackground(entryScope.rect, i);

                        GUILayout.Label(asset.Name, _labelStyle, GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT),
                            GUILayout.MinWidth(128f));

                        GUILayout.FlexibleSpace();

                        bool interactive = GUILayout.Toggle(asset.Interactive, "Interactive");
                        _configuration.SetInteractiveImportForAsset(asset.ID, interactive);

                        if (GUILayout.Button("Remove", _buttonStyle, GUILayout.Width(64f),
                                GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT)))
                        {
                            _configuration.DequeueAsset(asset.ID);

                            i--;
                            entriesCount--;
                        }
                    }

                    // Pages navigation
                    if (queuedAssets.Count > MAX_ENTRIES_PER_PAGE)
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
                            Mathf.CeilToInt(queuedAssets.Count / (float) MAX_ENTRIES_PER_PAGE);
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
    }
}