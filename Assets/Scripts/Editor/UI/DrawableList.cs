using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor.UI
{
    /// <summary>
    /// Draws list in a specific way:
    /// <list type="bullet">
    /// <item>Draws elements with alternating background.</item>
    /// <item>Draws a certain number of elements per page.</item>
    /// <item>Pages ui is hidden until needed.</item>
    /// <item>The entire list is drawn inside a container of "window" style.</item>
    /// <item>When the number of elements changes the pages must adjust accordingly.</item>
    /// <item>Elements can't be added or removed directly.</item>
    /// </list>
    /// </summary>
    internal class ListDrawer
    {
        private const int MAX_ENTRIES_PER_PAGE = 10;
        
        private GUIStyle _windowStyle;
        private GUIStyle _scopeStyle;
        private GUIStyle _labelStyle;

        private bool _stylesInitialized;
        
        private int _page = 1;


        public void Reset()
        {
            _page = 1;
        }
        
        
        public void Draw<T>(IList<T> list, Action<T> elementDrawFunction, string title)
        {
            if (!_stylesInitialized)
            {
                _windowStyle = new GUIStyle(GUI.skin.window);
                _scopeStyle = new GUIStyle();
                _labelStyle = new GUIStyle(GUI.skin.label);

                _stylesInitialized = true;
            }

            // Workaround to avoid index out of range exceptions if the element is removed from the list
            // in elementDrawFunction.
            list = list.ToList();
            
            using (new GUILayout.VerticalScope(title, _windowStyle))
            {
                if (list.Count <= 0)
                {
                    SetupWindowElements.DrawEmptyListElement();
                    return;
                }

                int start = (_page - 1) * MAX_ENTRIES_PER_PAGE;
                int entriesOnPage = Math.Min(MAX_ENTRIES_PER_PAGE, list.Count - start);
                if (entriesOnPage <= 0)
                {
                    _page--;
                    
                    start = (_page - 1) * MAX_ENTRIES_PER_PAGE;
                    entriesOnPage = Math.Min(MAX_ENTRIES_PER_PAGE, list.Count - start);
                }
                int end = start + entriesOnPage;
                
                for (int i = start; i < end; i++)
                {
                    using EditorGUILayout.HorizontalScope entryScope = new EditorGUILayout.HorizontalScope(_scopeStyle);
                    SetupWindowElements.DrawListElementBackground(entryScope.rect, i);
                    elementDrawFunction?.Invoke(list[i]);
                }

                DrawPagesNavigation(list.Count);
            }
        }


        private void DrawPagesNavigation(int totalElements)
        {
            if (totalElements < MAX_ENTRIES_PER_PAGE)
            {
                return;
            }
            
            using GUILayout.HorizontalScope navigationScope = new GUILayout.HorizontalScope(_scopeStyle);
            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledGroupScope(_page <= 1))
            {
                if (GUILayout.Button("<"))
                {
                    _page--;
                }
            }

            int maxPages =
                Mathf.CeilToInt(totalElements / (float) MAX_ENTRIES_PER_PAGE);
            GUILayout.Label($"{_page}/{maxPages}", _labelStyle);

            using (new EditorGUI.DisabledGroupScope(_page >= maxPages))
            {
                if (GUILayout.Button(">"))
                {
                    _page++;
                }
            }
        }
    }
}