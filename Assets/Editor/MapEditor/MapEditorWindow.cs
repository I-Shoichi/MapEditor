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
        Vector2 scrollPosition = new Vector2(0, 0);                 //! どこまでスクロールしたかを取得するポジション
        MapCanvas canvas;
        /*= ユーザーの初期設定 =============================================*/
        Object dataDirectory;                                       //! 使用するオブジェクトが入っているディレクトリ
        GameObject outputEmptyObject;                               //! 作成したマップデータを保管するオブジェクト
        Vector2 mapSize = new Vector2(10, 10);                      //! マップサイズを保管
        Vector3 partsSize = new Vector3(1, 1, 1);                   //!使用するオブジェクトのサイズを予め記述し、サイズの成型を行う
        SearchOption searchOption;                                  //! ファイルの検索範囲
        List<GameObject> partsObjects = new List<GameObject>();     //! 素材となるオブジェクト

        /*= Developer Settings =============================================*/
        const string WINDOW_NAME = "Map Editor"; //! タブに表示される名前
        const string VERSION = "1.0.0";          //! このスクリプトのバージョン

        /*= Font Size =============================================*/
        const int TITLE_FONT_SIZE = 32;          //! タイトルに使用するフォントサイズ
        const int HEADER_FONT_SIZE = 20;         //! ヘッダーに使用するフォントサイズ


        /// <summary>
        /// Windowの生成
        /// </summary>
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
            EditorGUILayout.Space();
            //タイトル
            Title();
            GUISupport.DrawLineHoraizontal();

            //input
            Header("Input");
            EditorGUILayout.Space();

            LoadObjectDirectory();

            LoadDirectoryOption();

            MapSize();
            GUISupport.DrawLineHoraizontal();

            //OutPot
            Header("Output");
            EditorGUILayout.Space();

            SetEmptyObject();

            EditorGUILayout.HelpBox("The input object can't have any children if it has them.", MessageType.Info);
            EditorGUILayout.Space();

            GUISupport.DrawLineHoraizontal();

            //Start
            Header("Start");
            OpenEditor();

            GUISupport.DrawLineHoraizontal();

            ViewLoadObjects();
        }

        /// <summary>
        /// タイトルを表記する
        /// </summary>
        private void Title()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUISupport.Centered(TITLE_FONT_SIZE, WINDOW_NAME);
            }
            EditorGUILayout.Space();
        }

        /// <summary>
        /// オブジェクトを読み込むフィールド
        /// </summary>
        private void LoadObjectDirectory()
        {
            using (new GUILayout.HorizontalScope())
            {
                Object tmp = null;

                //dataDirectoryがあるなら比較用に保管
                if (dataDirectory) tmp = dataDirectory;

                GUILayout.Label("Stage Resource File*", GUILayout.Width(150));

                //オブジェクトの代入
                dataDirectory = EditorGUILayout.ObjectField(dataDirectory, typeof(Object), true);

                //tmpが空でなければ、置換処理
                if (tmp)
                {
                    //入力されてるデータが変われば、リストの要素をリセットする
                    if (!tmp.Equals(dataDirectory)) partsObjects.Clear();
                }
            }
            EditorGUILayout.Space();
        }

        /// <summary>
        /// オブジェクトの読み込みの設定
        /// </summary>
        private void LoadDirectoryOption()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("File Load Option", GUILayout.Width(150));
                searchOption = (SearchOption)EditorGUILayout.EnumPopup(searchOption);
            }
        }

        /// <summary>
        /// マップサイズの指定
        /// </summary>
        private void MapSize()
        {
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
        }

        /// <summary>
        /// 出力する空オブジェクトを設定する
        /// </summary>
        private void SetEmptyObject()
        {
            //代入用のオブジェクト
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Empty Object*", GUILayout.Width(150));
                outputEmptyObject = EditorGUILayout.ObjectField(outputEmptyObject, typeof(GameObject), true) as GameObject;

                //オブジェクトがあるならTrue
                if (outputEmptyObject)
                {
                    //ゲームオブジェクトに子があるなら、空にする
                    if (outputEmptyObject.transform.childCount != 0)
                    {
                        outputEmptyObject = null;
                    }
                }
            }
            EditorGUILayout.Space();
        }

        /// <summary>
        /// エディタを開く
        /// </summary>
        private void OpenEditor()
        {
            if (GUILayout.Button("Open Editor"))
            {
                //Sub Windowがなければ生成する
                if (canvas == null)
                {
                    canvas = new MapCanvas(outputEmptyObject, partsObjects, mapSize);
                }

                //必要なデータがなければ、Windowを起動しない
                if (dataDirectory == null || outputEmptyObject == null)
                {
                    Debug.Log("No \"StageResourceFile\" or \"Empty Object\" was entered.");
                    return;
                }

                //Windowの表示
                canvas.Show();
                //ウィンドウを手前に表示
                canvas.Focus();
            }
            EditorGUILayout.Space();
        }

        /// <summary>
        /// 読み込んだオブジェクトを並べる
        /// </summary>
        private void ViewLoadObjects()
        {
            //ここから、スクロールビュー
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            //ファイルがないなら、何も表示しない
            if (dataDirectory)
            {
                // 指定されたオブジェクトのパスを取得
                string path = AssetDatabase.GetAssetOrScenePath(dataDirectory);

                try
                {
                    //Pathのディレクトリに含まれている*.prefab形式のデータを取得する
                    string[] objectChild = Directory.GetFiles(path, "*.prefab", searchOption);

                    for (int i = 0; i < objectChild.Length; i++)
                    {
                        //正規表現を使用して、オブジェクト形式のみ表示
                        Match match = Regex.Match(objectChild[i], "[_ a-zA-Z0-9]*.prefab");

                        //横に並べる
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(match.Value, GUILayout.Width(300));

                            //partsObjectsがnullかを判定
                            if (partsObjects == null) return;

                            if (partsObjects.IndexOf(AssetDatabase.LoadAssetAtPath<GameObject>(objectChild[i])) == -1)
                            {
                                //オブジェクトを代入する
                                partsObjects.Add(AssetDatabase.LoadAssetAtPath<GameObject>(objectChild[i]));
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.Log("This file format is not supported. Enter another piece of data.¥n" + e);
                    dataDirectory = null;
                }
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// ヘッダーを表示する
        /// </summary>
        /// <param name="text"></param>
        private void Header(string text)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUISupport.SetFontSizeLabel(HEADER_FONT_SIZE, text);
            }
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