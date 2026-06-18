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

        private int _availableAssetsPage = 1;
        private int _queuedAssetsPage = 1;

        private string _assetsSearchString;
        

        public AssetsSettingsUI(SetupConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void ResetTemporaryState()
        {
            _availableAssetsPage = 1;
            _queuedAssetsPage = 1;
            _assetsSearchString = string.Empty;
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
            _assetsSearchString =
                GUILayout.TextField(_assetsSearchString, _searchBarStyle,
                    GUILayout.MaxWidth(256f));
            
            List<string> availableAssetIDs = _configuration.AvailableAssets;
            if (!string.IsNullOrWhiteSpace(_assetsSearchString))
            {
                availableAssetIDs = _configuration.FindAssets(_assetsSearchString);

                _availableAssetsPage = 1;
            }
            
            // Available list
            List<AssetImportEntry> queuedAssets = _configuration.GetQueuedAssets();
            using (new GUILayout.VerticalScope($"Available ({availableAssetIDs.Count})", _windowStyle))
            {
                if (availableAssetIDs.Count > 0)
                {
                    int start = (_availableAssetsPage - 1) * MAX_ENTRIES_PER_PAGE;
                    int entriesCount = Math.Min(MAX_ENTRIES_PER_PAGE,
                        availableAssetIDs.Count - (_availableAssetsPage - 1) * MAX_ENTRIES_PER_PAGE);
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
                            
                    using (new EditorGUI.DisabledGroupScope(_availableAssetsPage <= 1))
                    {
                        if (GUILayout.Button("<"))
                        {
                            _availableAssetsPage--;
                        }
                    }
                            
                    int maxPages =
                        Mathf.CeilToInt(availableAssetIDs.Count / (float) MAX_ENTRIES_PER_PAGE);
                    GUILayout.Label($"{_availableAssetsPage}/{maxPages}", _labelStyle);
                            
                    using (new EditorGUI.DisabledGroupScope(_availableAssetsPage >= maxPages))
                    {
                        if (GUILayout.Button(">"))
                        {
                            _availableAssetsPage++;
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
                    int start = (_queuedAssetsPage - 1) * MAX_ENTRIES_PER_PAGE;
                    int entriesCount = Math.Min(MAX_ENTRIES_PER_PAGE,
                        queuedAssets.Count - (_queuedAssetsPage - 1) * MAX_ENTRIES_PER_PAGE);
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

                        using (new EditorGUI.DisabledGroupScope(_queuedAssetsPage <= 1))
                        {
                            if (GUILayout.Button("<"))
                            {
                                _queuedAssetsPage--;
                            }
                        }

                        int maxPages =
                            Mathf.CeilToInt(queuedAssets.Count / (float) MAX_ENTRIES_PER_PAGE);
                        GUILayout.Label($"{_queuedAssetsPage}/{maxPages}", _labelStyle);

                        using (new EditorGUI.DisabledGroupScope(_queuedAssetsPage >= maxPages))
                        {
                            if (GUILayout.Button(">"))
                            {
                                _queuedAssetsPage++;
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