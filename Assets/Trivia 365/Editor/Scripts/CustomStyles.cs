using UnityEngine;
using UnityEditor;

namespace QuizApp.Editor
{
    public class CustomStyles
    {
        public static GUISkin inspectorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

        private static GUIStyle toolbarButton = EditorStyles.toolbarButton;
        private static GUIStyle toolbarDropDown = EditorStyles.toolbarDropDown;
        private static GUIStyle helpBox = EditorStyles.helpBox;

        public static GUIStyle headerAttribute = new GUIStyle(toolbarButton)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            richText = true,

            fixedHeight = 20
        };

        private static GUIStyle m_background = new GUIStyle()
        {
            margin = new RectOffset(2, 2, 2, 2),
        };

        public static GUIStyle m_boldLabel = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            richText = true
        };

        public static GUIStyle m_centerBoldLabel = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            richText = true
        };

        public static GUIStyle m_rightBoldLabel = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleRight,
            richText = true
        };

        public static GUIStyle m_xlTabs = new GUIStyle(toolbarButton)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,

            fixedHeight = 35
        };

        public static GUIStyle m_standardBtn = new GUIStyle(inspectorSkin.button)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,

            fixedHeight = 30
        };

        public static GUIStyle m_toolbarBtn = new GUIStyle(toolbarButton)
        {
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,

            fixedHeight = 20
        };

        public static GUIStyle m_xlToolbarBtn = new GUIStyle(toolbarButton)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,

            fixedHeight = 25
        };

        public static GUIStyle m_toolbarDropdown = new GUIStyle(toolbarDropDown)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,

            fixedHeight = 25
        };

        public static GUIStyle m_textField = new GUIStyle(inspectorSkin.textField)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft,

            fixedHeight = 22
        };

        public static GUIStyle m_helpBox = new GUIStyle(helpBox)
        {
            fontSize = 11,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,

            richText = true,
            fixedHeight = 20
        };

        public static GUIStyle m_centeredHelpBox = new GUIStyle(helpBox)
        {
            fontSize = 11,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,

            richText = true,
            fixedHeight = 22
        };

        public static Texture2D MakeTex(Color col)
        {
            Color[] pix = new Color[1 * 1];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            result.hideFlags = HideFlags.HideAndDontSave;
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        public static GUIStyle m_darkBackground()
        {
            m_background.normal.background = MakeTex(new Color(0.3f, 0.3f, 0.3f, 0.25f));

            return m_background;
        }
    }
}