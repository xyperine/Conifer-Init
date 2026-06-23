using System.Collections.Generic;
using ProjectSetup.Editor.Configuration;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor.UI
{
    internal sealed class AssetsSettingsUI
    {
        private readonly SetupConfiguration _configuration;
        
        private readonly ListDrawer _availableListDrawer;
        private readonly ListDrawer _queuedListDrawer;

        private GUIStyle _titleStyle;
        private GUIStyle _searchBarStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;

        private bool _stylesInitialized;
        
        private string _searchString;
        

        public AssetsSettingsUI(SetupConfiguration configuration)
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
            
            SetupWindowElements.DrawRegularSpace();
            
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
                                
                GUILayout.Label(assetName, _labelStyle, GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT), GUILayout.MinWidth(128f));
                if (GUILayout.Button("Import", _buttonStyle, GUILayout.Width(64f), GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT)))
                {
                    _configuration.QueueAsset(id);
                }
            }, $"Available ({availableAssetIDs.Count})");
            
            SetupWindowElements.DrawRegularSpace();
                
            // Queued list
            List<AssetImportEntry> queuedAssets = _configuration.GetQueuedAssets();
            _queuedListDrawer.Draw(queuedAssets, entry =>
            {
                GUILayout.Label(entry.Name, _labelStyle, GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT),
                    GUILayout.MinWidth(128f));

                GUILayout.FlexibleSpace();

                bool interactive = GUILayout.Toggle(entry.Interactive, "Interactive");
                _configuration.SetInteractiveImportForAsset(entry.ID, interactive);

                if (GUILayout.Button("Remove", _buttonStyle, GUILayout.Width(64f),
                        GUILayout.Height(SetupWindowElements.REGULAR_ELEMENT_HEIGHT)))
                {
                    _configuration.DequeueAsset(entry.ID);
                }
            }, $"Queued ({queuedAssets.Count})");
        }
    }
}