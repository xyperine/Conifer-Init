using UnityEngine;

namespace ProjectSetup.Editor
{
    public static class SetupWindowElements
    {
        public const float SMALL_SPACE_SIZE = 4f;
        public const float REGULAR_SPACE_SIZE = 8f;
        
        
        public static void DrawEmptyListElement()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("—");
                GUILayout.FlexibleSpace();
            }
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