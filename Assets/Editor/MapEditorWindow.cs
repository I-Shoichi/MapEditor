using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

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

        SearchOption searchOption;  //! ファイルの検索範囲
        List<GameObject> partsObjects; //! 素材となるオブジェクト

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

            #region ### Input ###
            //ヘッダー
            using (new GUILayout.HorizontalScope())
            {
                FontSizeChangeLabel("Input", HEADER_FONT_SIZE);
            }
            EditorGUILayout.Space();

            //オブジェクトファイルの読み込み
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Stage Resource File*", GUILayout.Width(150));
                dataDirectory = EditorGUILayout.ObjectField(dataDirectory, typeof(Object), true);
            }
            EditorGUILayout.Space();

            //読み込むファイルの範囲を選択
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("File Load Option", GUILayout.Width(150));
                searchOption = (SearchOption)EditorGUILayout.EnumPopup(searchOption);
            }

            //マップサイズの指定
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Map Size", GUILayout.Width(150));
                GUILayout.Label("X : ");
                mapSize.x = EditorGUILayout.FloatField(mapSize.x);
                MapSizeCheck(ref mapSize.x);

                GUILayout.Label("Y : ");
                mapSize.y = EditorGUILayout.FloatField(mapSize.y);
                MapSizeCheck(ref mapSize.y);

            }
            EditorGUILayout.Space();

            DrawLine();
            #endregion

            #region ### Output ###

            #endregion

            #region ### Input Objects Check Space ###
            //ここから、スクロールビュー
            EditorGUILayout.BeginScrollView(new Vector2(0, 0), GUI.skin.box);
            
            //ファイルがないなら、何も表示しない
            if (dataDirectory)
            {
                // 指定されたオブジェクトのパスを取得
                string path = AssetDatabase.GetAssetOrScenePath(dataDirectory);

                //Pathのディレクトリに含まれている*.prefab形式のデータを取得する
                string[] objectChild = Directory.GetFiles(path, "*.prefab", searchOption);

                for(int i = 0; i < objectChild.Length; i++)
                {
                    //正規表現を使用して、オブジェクト形式のみ表示
                    Match match = Regex.Match(objectChild[i], "[a-zA-Z0-9]*.prefab");
                    GUILayout.Label(match.Value);
                }
            }
            EditorGUILayout.EndScrollView();
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

        /// <summary>
        /// マップサイズが指定可能な状態かを確認する
        /// <para>絶対値, 0->1, 5捨6入</para>
        /// </summary>
        /// <param name="value"></param>
        private void MapSizeCheck(ref float value)
        {
            //絶対値に
            value = Mathf.Abs(value);

            //0以下なら１に
            if (value <= 0) value = 1;

            //四捨五入
            value = Mathf.RoundToInt(value);
        }
    }
}
