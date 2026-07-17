using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor.UI
{
    // Excluded styles:
    // - Window title
    
    /// <summary>
    /// Contains the majority of styles used in the window UI. Note that it provides original style objects,
    /// not their copies.
    /// </summary>
    internal class Styles
    {
        public bool Initialized { get; private set; }
        
        public GUIStyle EntireWindow { get; private set; }
        public GUIStyle List { get; private set; }
        public GUIStyle Scope { get; private set; }
        public GUIStyle SectionTitle { get; private set; }
        public GUIStyle Button { get; private set; }
        public GUIStyle Label { get; private set; }
        public GUIStyle SearchBar { get; private set; }
        public GUIStyle Popup { get; private set; }
        public GUIStyle SuccessMessage { get; private set; }
        public GUIStyle ErrorMessage { get; private set; }
        public GUIStyle HoverableLabel { get; private set; }
        public GUIStyle ExecuteButton { get; private set; }


        public void Initialize()
        {
            Initialized = true;
            
            Scope = new GUIStyle();
            
            ExecuteButton =  new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
            };
            
            SectionTitle = new GUIStyle(EditorStyles.boldLabel);
            Button = new GUIStyle(GUI.skin.button);
            List = new GUIStyle(GUI.skin.window);
            Label = new GUIStyle(GUI.skin.label);
            
#if UNITY_6000_1_OR_NEWER
            Color hoveredLabelColor = Color.cornflowerBlue; 
#else
            Color hoveredLabelColor = new Color(0.3921569f, 0.5843138f, 0.9294118f, 1f);
#endif
            
            HoverableLabel = new GUIStyle(GUI.skin.label)
            {
                hover = new GUIStyleState() {textColor = hoveredLabelColor},
                active = new GUIStyleState() {textColor = hoveredLabelColor},
            };
            
            SearchBar = new GUIStyle(EditorStyles.toolbarSearchField);
            
#if UNITY_6000_1_OR_NEWER
            Color successColor = Color.limeGreen;
#else
            Color successColor = new Color(0.1960784f, 0.8039216f, 0.1960784f, 1f);
#endif
            SuccessMessage = new GUIStyle(GUI.skin.label);
            SuccessMessage.normal = new GUIStyleState() {textColor = successColor};
            SuccessMessage.hover = new GUIStyleState() {textColor = successColor};
            SuccessMessage.active = new GUIStyleState() {textColor = successColor};
            SuccessMessage.focused = new GUIStyleState() {textColor = successColor};
            SuccessMessage.wordWrap = true;
            
#if UNITY_6000_1_OR_NEWER
            Color errorColor = Color.crimson;
#else
            Color errorColor = new Color(0.8627452f, 0.07843138f, 0.2352941f, 1);
#endif
            ErrorMessage = new GUIStyle(GUI.skin.label);
            ErrorMessage.normal = new GUIStyleState() {textColor = errorColor};
            ErrorMessage.hover = new GUIStyleState() {textColor = errorColor};
            ErrorMessage.active = new GUIStyleState() {textColor = errorColor};
            ErrorMessage.focused = new GUIStyleState() {textColor = errorColor};
            ErrorMessage.wordWrap = true;

            Popup = new GUIStyle(EditorStyles.popup);
            EntireWindow =  new GUIStyle
            {
                padding = new RectOffset(16, 16, 16, 16),
            };
        }
    }
}