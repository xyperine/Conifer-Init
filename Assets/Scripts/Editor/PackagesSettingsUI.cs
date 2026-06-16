using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ProjectSetup.Editor
{
    public class PackagesSettingsUI
    {
        private readonly SetupBusiness _business;
        
        private int _availablePackagesPage = 1;
        private int _queuedPackagesPage = 1;

        private string _packagesSearchString;


        public PackagesSettingsUI(SetupBusiness business)
        {
            _business = business;
        }


        public void Reset()
        {
            _packagesSearchString = string.Empty;
        }
        
        
        public void Draw()
        {
            const int maxEntriesPerPage = 10;
            
            GUILayout.Label("Packages Settings", new GUIStyle(EditorStyles.boldLabel));

            if (!SuccessfullyRetrievedPackages())
            {
                return;
            }

            // Search feature
            _packagesSearchString =
                GUILayout.TextField(_packagesSearchString, new GUIStyle(EditorStyles.toolbarSearchField),
                    GUILayout.MaxWidth(256f));
            
            List<string> availablePackageIDs = _business.AvailablePackages;
            if (!string.IsNullOrWhiteSpace(_packagesSearchString))
            {
                availablePackageIDs = _business.FindPackages(_packagesSearchString);

                _availablePackagesPage = 1;
            }
            
            // Available list
            List<string> queuedPackageIDs = _business.GetQueuedPackageIDs();
            using (new GUILayout.VerticalScope($"Available ({availablePackageIDs.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (availablePackageIDs.Count > 0)
                {
                    int start = (_availablePackagesPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        availablePackageIDs.Count - (_availablePackagesPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        PackageInfo packageInfo = _business.GetPackageByID(availablePackageIDs[i]);
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
                                
                        GUILayout.Label(packageInfo.displayName, new GUIStyle(GUI.skin.label), GUILayout.Height(16f), GUILayout.MinWidth(128f));
                        if (GUILayout.Button("Import", new GUIStyle(GUI.skin.button), GUILayout.Width(64f), GUILayout.Height(16f)))
                        {
                            _business.QueuePackage(availablePackageIDs[i]);

                            i--;
                        }
                    }

                    // Pages navigation
                    using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(new GUIStyle());
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
                    GUILayout.Label($"{_availablePackagesPage}/{maxPages}", new GUIStyle(GUI.skin.label));
                            
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

            SetupWindowElements.DrawRegularSpace();
                
            // Queued list
            using (new GUILayout.VerticalScope($"Queued ({queuedPackageIDs.Count})", new GUIStyle(GUI.skin.window)))
            {
                if (queuedPackageIDs.Count > 0)
                {
                    int start = (_queuedPackagesPage - 1) * maxEntriesPerPage;
                    int entriesCount = Math.Min(maxEntriesPerPage,
                        queuedPackageIDs.Count - (_queuedPackagesPage - 1) * maxEntriesPerPage);
                    for (int i = start; i < start + entriesCount; i++)
                    {
                        PackageInfo packageInfo = _business.GetPackageByID(queuedPackageIDs[i]);
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

                        GUILayout.Label(packageInfo.displayName, new GUIStyle(GUI.skin.label), GUILayout.Height(16f),
                            GUILayout.MinWidth(128f));

                        if (GUILayout.Button("Remove", new GUIStyle(GUI.skin.button), GUILayout.Width(64f),
                                GUILayout.Height(16f)))
                        {
                            _business.DequeuePackage(queuedPackageIDs[i]);

                            i--;
                            entriesCount--;
                        }
                    }

                    // Pages navigation
                    if (queuedPackageIDs.Count > maxEntriesPerPage)
                    {
                        using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(new GUIStyle());
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
                        GUILayout.Label($"{_queuedPackagesPage}/{maxPages}", new GUIStyle(GUI.skin.label));

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
            switch (_business.PackagesListRequest.Status)
            {
                case StatusCode.InProgress:
                    GUILayout.Label("Retrieving packages...");
                    break;
                case StatusCode.Success:
                    //Debug.Log("Successfully retrieved packages.");
                    GUIStyle style1 = new GUIStyle(GUI.skin.label)
                        {normal = new GUIStyleState() {textColor = Color.limeGreen}};
                    GUILayout.Label($"Retrieved packages: {_business.PackagesListRequest.Result.Length}", style1);
                    break;
                case StatusCode.Failure:
                    //Debug.Log("Failed to retrieve packages.");
                    GUIStyle style = new GUIStyle(GUI.skin.label)
                        {normal = new GUIStyleState() {textColor = Color.crimson}};
                    GUILayout.Label($"Error while retrieving packages: {_business.PackagesListRequest.Error.message}", style);
                    break;
                default:
                    Debug.LogError("Invalid request");
                    throw new ArgumentOutOfRangeException();
            }
            
            return _business.SuccessfullyRetrievedPackages();
        }
    }
}