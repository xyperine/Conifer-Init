using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ConiferInit.Editor.UI
{
    internal sealed class FooterUI
    {
        private readonly Styles _styles;


        public FooterUI(Styles styles)
        {
            _styles = styles;
        }
        
        
        public void Draw()
        {
            const string githubUrl = "https://github.com/xyperine";
            const string itchIoUrl = "https://xyperine.itch.io/";
            const string sourceCodeUrl = "https://github.com/xyperine/Conifer-Init";
            const string userGuidedUrl = "https://github.com/xyperine/Conifer-Init/blob/main/User Guide.pdf";
            const string userGuideLocalPath = "Assets/Plugins/ConiferInit/User Guide.pdf";
            // DON'T FORGET TO UPDATE THIS WHENEVER THE VERSION CHANGES
            const string version = "0.1.1";
            
            using var s = new GUILayout.HorizontalScope(_styles.Scope);

            if (EditorGUILayout.LinkButton("User Guide"))
            {
                if (AssetDatabase.AssetPathExists(userGuideLocalPath))
                {
                    string userGuideFullPath = Path.Combine(System.Environment.CurrentDirectory, userGuideLocalPath);
                    Process.Start(userGuideFullPath);
                }
                else
                {
                    Application.OpenURL(userGuidedUrl);
                }
            }

            WindowElements.DrawRegularSpace();
            
            if (EditorGUILayout.LinkButton("Source Code"))
            {
                Application.OpenURL(sourceCodeUrl);
            }
            
            WindowElements.DrawRegularSpace();

            if (EditorGUILayout.LinkButton("GitHub"))
            {
                Application.OpenURL(githubUrl);
            }

            WindowElements.DrawRegularSpace();

            if (EditorGUILayout.LinkButton("itch.io"))
            {
                Application.OpenURL(itchIoUrl);
            }

            GUILayout.FlexibleSpace();
            
            GUILayout.Label(version);
        }
    }
}