using System.Collections.Generic;
using ConiferInit.Editor.Configuration;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor.UI
{
    internal sealed class AssetsSettingsUI
    {
        private readonly AssetsSettingsConfiguration _configuration;
        private readonly Styles _styles;
        
        private readonly ListDrawer _availableListDrawer;
        private readonly ListDrawer _queuedListDrawer;
        
        private string _searchString;
        

        public AssetsSettingsUI(AssetsSettingsConfiguration configuration, Styles styles)
        {
            _configuration = configuration;
            _styles = styles;

            _availableListDrawer = new ListDrawer(_styles);
            _queuedListDrawer = new ListDrawer(_styles);
        }


        public void ResetTemporaryState()
        {
            _availableListDrawer.Reset();
            _queuedListDrawer.Reset();
            
            _searchString = string.Empty;
        }
        
        
        public void Draw()
        {
            GUILayout.Label("Assets Settings", _styles.SectionTitle);

            if (!_configuration.SuccessfullyRetrievedAssets)
            {
                _configuration.RetrieveCachedAssets();
                
                return;
            }
            
            // Search feature
            _searchString = GUILayout.TextField(_searchString, _styles.SearchBar, GUILayout.MaxWidth(256f));
            
            WindowElements.DrawRegularSpace();
            
            List<string> availableAssetIDs = _configuration.AvailableAssets;
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                availableAssetIDs = _configuration.FindAssets(_searchString);

                _availableListDrawer.Reset();
            }
            
            // Available list
            _availableListDrawer.Draw(availableAssetIDs, id =>
            {
                string assetName = _configuration.FindAssetByID(id).Name;
                                
                GUILayout.Label(assetName, _styles.Label, GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT), GUILayout.MinWidth(128f));
                if (GUILayout.Button("Import", _styles.Button, GUILayout.Width(64f), GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT)))
                {
                    _configuration.QueueAsset(id);
                }
            }, $"Available ({availableAssetIDs.Count})");
            
            WindowElements.DrawRegularSpace();
                
            // Queued list
            List<AssetImportEntry> queuedAssets = _configuration.GetQueuedAssets();
            _queuedListDrawer.Draw(queuedAssets, entry =>
            {
                GUILayout.Label(entry.Name, _styles.Label, GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT),
                    GUILayout.MinWidth(128f));

                GUILayout.FlexibleSpace();

                bool interactive = GUILayout.Toggle(entry.Interactive, "Interactive");
                _configuration.SetInteractiveImportForAsset(entry.ID, interactive);

                if (GUILayout.Button("Remove", _styles.Button, GUILayout.Width(64f),
                        GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT)))
                {
                    _configuration.DequeueAsset(entry.ID);
                }
            }, $"Queued ({queuedAssets.Count})");
        }
    }
}