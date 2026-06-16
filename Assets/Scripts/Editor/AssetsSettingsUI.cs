using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor
{
    public class AssetsSettingsUI
    {
        private readonly SetupConfiguration _configuration;

        private int _availableAssetsPage = 1;
        private int _queuedAssetsPage = 1;

        private string _assetsSearchString;
        

        public AssetsSettingsUI(SetupConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void ResetTemporaryState()
        {
            _assetsSearchString = string.Empty;
        }
        
        
        public void Draw()
        { 
            const int maxEntriesPerPage = 10;
            
            GUILayout.Label("Assets Settings", new GUIStyle(EditorStyles.boldLabel));

            if (!_configuration.SuccessfullyRetrievedAssets)
            {
                _configuration.RetrieveCachedAssets();
                
                return;
            }
            
            // Search feature
            _assetsSearchString =
                GUILayout.TextField(_assetsSearchString, new GUIStyle(EditorStyles.toolbarSearchField),
                    GUILayout.MaxWidth(256f));
            
            List<string> availableAssetIDs = _configuration.AvailableAssets;
            if (!string.IsNullOrWhiteSpace(_assetsSearchString))
            {
                availableAssetIDs = _configuration.FindAssets(_assetsSearchString);

                _availableAssetsPage = 1;
            }
            
            // Available list
            List<AssetImportEntry> queuedAssets = _configuration.GetQueuedAssets();
            using (new GUILayout.VerticalScope($"Available ({availableAssetIDs.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (availableAssetIDs.Count > 0)
                {
                    int start = (_availableAssetsPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        availableAssetIDs.Count - (_availableAssetsPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        string assetName = _configuration.FindAssetByID(availableAssetIDs[i]).Name;
                        using EditorGUILayout.HorizontalScope entryScope = new EditorGUILayout.HorizontalScope(new GUIStyle());
                        Color bgColor = i % 2 == 0 
                            ? new Color(0f, 0f, 0f, 0.03f)
                            : new Color(1f, 1f, 1f, 0.03f);

                        Rect rect = new Rect
                        {
                            position = entryScope.rect.position,
                            size = entryScope.rect.size,
                        };
                        EditorGUI.DrawRect(rect, bgColor);
                                
                        GUILayout.Label(assetName, new GUIStyle(GUI.skin.label), GUILayout.Height(16f), GUILayout.MinWidth(128f));
                        if (GUILayout.Button("Import", new GUIStyle(GUI.skin.button), GUILayout.Width(64f), GUILayout.Height(16f)))
                        {
                            _configuration.QueueAsset(availableAssetIDs[i]);

                            i--;
                        }
                    }

                    // Pages navigation
                    using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(new GUIStyle());
                    GUILayout.FlexibleSpace();
                            
                    using (new EditorGUI.DisabledGroupScope(_availableAssetsPage <= 1))
                    {
                        if (GUILayout.Button("<"))
                        {
                            _availableAssetsPage--;
                        }
                    }
                            
                    int maxPages =
                        Mathf.CeilToInt(availableAssetIDs.Count / (float) maxEntriesPerPage);
                    GUILayout.Label($"{_availableAssetsPage}/{maxPages}", new GUIStyle(GUI.skin.label));
                            
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
            using (new GUILayout.VerticalScope($"Queued ({queuedAssets.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (queuedAssets.Count > 0)
                {
                    int start = (_queuedAssetsPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        queuedAssets.Count - (_queuedAssetsPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        AssetImportEntry asset = queuedAssets[i];
                        using EditorGUILayout.HorizontalScope entryScope =
                            new EditorGUILayout.HorizontalScope(new GUIStyle());
                        Color bgColor = i % 2 == 0
                            ? new Color(0f, 0f, 0f, 0.03f)
                            : new Color(1f, 1f, 1f, 0.03f);

                        Rect rect = new Rect
                        {
                            position = entryScope.rect.position,
                            size = entryScope.rect.size,
                        };
                        EditorGUI.DrawRect(rect, bgColor);

                        GUILayout.Label(asset.Name, new GUIStyle(GUI.skin.label), GUILayout.Height(16f),
                            GUILayout.MinWidth(128f));

                        GUILayout.FlexibleSpace();

                        bool interactive = GUILayout.Toggle(asset.Interactive, "Interactive");
                        _configuration.SetInteractiveImportForAsset(asset.ID, interactive);

                        if (GUILayout.Button("Remove", new GUIStyle(GUI.skin.button), GUILayout.Width(64f),
                                GUILayout.Height(16f)))
                        {
                            _configuration.DequeueAsset(asset.ID);

                            i--;
                            entriesCount--;
                        }
                    }

                    // Pages navigation
                    if (queuedAssets.Count > maxEntriesPerPage)
                    {
                        using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(new GUIStyle());
                        GUILayout.FlexibleSpace();

                        using (new EditorGUI.DisabledGroupScope(_queuedAssetsPage <= 1))
                        {
                            if (GUILayout.Button("<"))
                            {
                                _queuedAssetsPage--;
                            }
                        }

                        int maxPages =
                            Mathf.CeilToInt(queuedAssets.Count / (float) maxEntriesPerPage);
                        GUILayout.Label($"{_queuedAssetsPage}/{maxPages}", new GUIStyle(GUI.skin.label));

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