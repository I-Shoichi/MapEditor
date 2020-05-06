using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MapEditor
{
    /// <summary>
    /// MapEditorを使用するためのWindowを作成する
    /// @author Shoichi Ikeda
    /// </summary>
    public class MapEditorWindow : EditorWindow
    {
        #region ### Globals ###
        /*ユーザーの初期設定*/
        Object dataDirectory; //! 使用するオブジェクトが入っているディレクトリ
        Object outputEmptyObject; //! 作成したマップデータを保管するオブジェクト
        Vector2 mapSize = new Vector2(10, 10); //! マップサイズを保管
        Vector3 partsSize = new Vector3(1, 1, 1); //!使用するオブジェクトのサイズを予め記述し、サイズの成型を行う

        /*Developer Settings*/
        const string WINDOW_NAME = "Map Editor"; //! タブに表示される名前
        const string VERSION = "1.0.0"; //! このスクリプトのバージョン

        const int TITLE_FONT_SIZE = 32; //! タイトルに使用するフォントサイズ
        const int HEADER_FONT_SIZE = 20; //! ヘッダーに使用するフォントサイズ
        #endregion

        [MenuItem("Window/" + WINDOW_NAME)]
        private static void Create()
        {
            GetWindow<MapEditorWindow>(WINDOW_NAME);
        }

        /// <summary>
        /// GUIの設定
        /// </summary>
        private void OnGUI()
        {
            #region ### Title ###
            EditorGUILayout.Space();
            //タイトルブロック
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                
                //タイトルを表記
                FontSizeChangeLabel(WINDOW_NAME, TITLE_FONT_SIZE);
                
                //バージョン表記
                GUILayout.Label(VERSION);

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.Space();

            DrawLine();
            #endregion


        }

        /// <summary>
        /// フォントサイズを変更したLabel
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        private void FontSizeChangeLabel(string text, int fontSize)
        {
            GUI.skin.label.fontSize = fontSize;
            GUILayout.Label(text);
            GUI.skin.label.fontSize = 0;
        }

        /// <summary>
        /// 横に線を描画する
        /// </summary>
        private void DrawLine()
        {
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
            EditorGUILayout.Space();
        }
    }
}
