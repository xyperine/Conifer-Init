using UnityEditor;
using UnityEngine;

namespace ProjectSetup.Editor.UI
{
    public static class SetupWindowElements
    {
        public const float SMALL_SPACE_SIZE = 4f;
        public const float REGULAR_SPACE_SIZE = 8f;
        public const float REGULAR_ELEMENT_HEIGHT = 16f;

        private static readonly Color ListElementColor1 = new Color(0f, 0f, 0f, 0.03f);
        private static readonly Color ListElementColor2 = new Color(1f, 1f, 1f, 0.03f);
        
        
        public static void DrawEmptyListElement()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("—");
                GUILayout.FlexibleSpace();
            }
        }


        public static void DrawListElementBackground(Rect entryRect, int elementIndex)
        {
            Color bgColor = elementIndex % 2 == 0 ? ListElementColor1 : ListElementColor2;

            Rect rect = new Rect
            {
                position = entryRect.position,
                size = entryRect.size,
            };
            
            EditorGUI.DrawRect(rect, bgColor);
        }


        public static void DrawRegularSpace()
        {
            GUILayout.Space(REGULAR_SPACE_SIZE);
        }


        public static void DrawSmallSpace()
        {
            GUILayout.Space(SMALL_SPACE_SIZE);
        }
    }
}