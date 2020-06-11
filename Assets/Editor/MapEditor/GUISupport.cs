using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MapEditor
{
    /// <summary>
    /// UnityEditorを開発する上で、サポートをしてくれる
    /// @author Shoichi Ikeda
    /// </summary>
    public static class GUISupport
    {
        /// <summary>
        /// 中央揃えで、文字列を表示する
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="texts"></param>
        public static void Centered(int fontSize = 10, params string[] texts)
        {
            GUILayout.FlexibleSpace();

            //指定されている文字列を表示する
            foreach (string text in texts)
            {
                SetFontSizeLabel(fontSize, text);
            }

            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// 文字列のサイズを指定されたフォントサイズに置き換えて表示する
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        public static void SetFontSizeLabel(int fontSize, string text)
        {
            //元々のフォントサイズを保管
            int originFontSize = GUI.skin.label.fontSize;

            //フォントサイズを指定のサイズに変更する
            GUI.skin.label.fontSize = fontSize;
            GUILayout.Label(text);

            //フォントサイズを元に戻す
            GUI.skin.label.fontSize = originFontSize;
        }

        /// <summary>
        /// 線を描画する
        /// </summary>
        public static void DrawLineHoraizontal()
        {
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
            EditorGUILayout.Space();
        }

		public static void DrawLineVertical()
		{
			GUILayout.Box("", GUILayout.Width(1), GUILayout.ExpandHeight(true));
		}
    }
}


