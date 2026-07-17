using UnityEditor;

namespace ConiferInit.Editor.UI
{
    /// <summary>
    /// Editor version-agnostic dialog.
    /// </summary>
    internal static class Dialog
    {
        public static bool DisplayDecisionDialog(string titleText, string message, string yesButtonText, string noButtonText)
        {
#if UNITY_6000_3_OR_NEWER
            return EditorDialog.DisplayDecisionDialog(titleText, message, yesButtonText, noButtonText);
#else
            return EditorUtility.DisplayDialog(titleText, message, yesButtonText, noButtonText);
#endif
        }
    }
}