using UnityEngine;

namespace ProjectSetup.Editor
{
    public static class SetupWindowElements
    {
        public const float SPACE_SIZE = 4f;
        
        
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
            GUILayout.Space(SPACE_SIZE);
        }
    }
}