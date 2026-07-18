using System;
using System.Collections.Generic;
using ConiferInit.Editor.Configuration;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ConiferInit.Editor.UI
{
    internal sealed class PackagesSettingsUI
    {
        private readonly PackagesSettingsConfiguration _configuration;
        private readonly Styles _styles;

        private readonly ListDrawer _availableListDrawer;
        private readonly ListDrawer _queuedListDrawer;

        private string _searchString;


        public PackagesSettingsUI(PackagesSettingsConfiguration configuration, Styles styles)
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
            GUILayout.Label("Packages Settings", _styles.SectionTitle);

            if (!SuccessfullyRetrievedPackages())
            {
                return;
            }

            // Search feature
            _searchString =
                GUILayout.TextField(_searchString, _styles.SearchBar, GUILayout.MaxWidth(256f));
            
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
                                
                GUILayout.Label(packageInfo.displayName, _styles.Label, GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT), GUILayout.MinWidth(128f));
                if (GUILayout.Button("Import", _styles.Button, GUILayout.Width(64f), GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT)))
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

                GUILayout.Label(packageInfo.displayName, _styles.Label, GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT),
                    GUILayout.MinWidth(128f));

                if (GUILayout.Button("Remove", _styles.Button, GUILayout.Width(64f),
                        GUILayout.Height(WindowElements.REGULAR_ELEMENT_HEIGHT)))
                {
                    _configuration.DequeuePackage(entry.ShortID);
                }
            }, $"Queued ({queuedPackages.Count})");
        }


        private bool SuccessfullyRetrievedPackages()
        {
            switch (_configuration.PackagesListRequest.Status)
            {
                case StatusCode.InProgress:
                    GUILayout.Label("Retrieving packages...");
                    break;
                case StatusCode.Success:
                    GUILayout.Label($"Retrieved packages: {_configuration.PackagesListRequest.Result.Length}",
                        _styles.SuccessMessage);
                    break;
                case StatusCode.Failure:
                    GUILayout.Label(
                        $"Error while retrieving packages: {_configuration.PackagesListRequest.Error.message}",
                        _styles.ErrorMessage);
                    break;
                default:
                    Debug.LogError("Invalid request");
                    throw new ArgumentOutOfRangeException();
            }
            
            return _configuration.SuccessfullyRetrievedPackages();
        }
    }
}