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
        MapSubEditorWindow subWindow;
        Vector2 scrollPosition = new Vector2(0, 0); //! どこまでスクロールしたかを取得するポジション

        /*ユーザーの初期設定*/
        Object dataDirectory; //! 使用するオブジェクトが入っているディレクトリ
        GameObject outputEmptyObject; //! 作成したマップデータを保管するオブジェクト
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
                Object tmp = null;

                //dataDirectoryがあるなら比較用に保管
                if(dataDirectory) tmp = dataDirectory;
                GUILayout.Label("Stage Resource File*", GUILayout.Width(150));
                dataDirectory = EditorGUILayout.ObjectField(dataDirectory, typeof(Object), true);

                //入力されてるデータが変われば、リストの要素をリセットする
                if(tmp != dataDirectory) partsObjects.Clear();
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
            //ヘッダー
            using (new GUILayout.HorizontalScope())
            {
                FontSizeChangeLabel("Output", HEADER_FONT_SIZE);
            }
            EditorGUILayout.Space();

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

            GUILayout.Label("※The input object can't have any children if it has them.");
            EditorGUILayout.Space();

            DrawLine();
            #endregion

            #region ### Start ###
            //ヘッダー
            using (new GUILayout.HorizontalScope())
            {
                FontSizeChangeLabel("Start", HEADER_FONT_SIZE);
            }
            EditorGUILayout.Space();

            //エディタを起動するボタン
            if (GUILayout.Button("Open Editor"))
            {
                //Sub Windowがなければ生成する
                if (subWindow == null)
                {
                    subWindow = new MapSubEditorWindow(ref outputEmptyObject, partsObjects, mapSize);
                }

                //必要なデータがなければ、Windowを起動しない
                if (dataDirectory == null || outputEmptyObject == null)
                {
                    Debug.Log("No \"StageResourceFile\" or \"Empty Object\" was entered.");
                    return;
                }

                //Windowの表示
                subWindow.Show();
                //ウィンドウを手前に表示
                subWindow.Focus();
            }
            EditorGUILayout.Space();

            DrawLine();
            #endregion

            #region ### Input Objects Check Space ###
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
                        Match match = Regex.Match(objectChild[i], "[ a-zA-Z0-9]*.prefab");

                        //横に並べる
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(match.Value, GUILayout.Width(300));

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
                    Debug.Log("This file format is not supported. Enter another piece of data.");
                    dataDirectory = null;
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

    /// <summary>
    /// マップのサブウィンドウを表示する
    /// @author Shoichi Ikeda
    /// </summary>
    public class MapSubEditorWindow : EditorWindow
    {
        Vector2 scrollPosition = new Vector2(0, 0); //! スクロール幅
        GridCell[,] cell; //! セルデータ
        MouseEvents events = MouseEvents.none; //! マウスデータを初期化
        Pallet pallet; //! パレットのデータ

        /*ユーザー設定*/
        Color gridColor = Color.white; //! 線の色
        Color backGroundColor = new Vector4(0.0f, 0.6f, 1.0f, 1.0f); //! 背景の色
        
        /*Init Input Datas*/
        GameObject saveObject;
        List<GameObject> partsObject;
        Vector2 mapSize;
        
        /*Developer Settings*/
        const string WINDOW_NAME = "Editor"; //! タブに表示される名前
        float gridSize = 30; //! grid線のサイズ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="empty"></param>
        /// <param name="parts"></param>
        /// <param name="map"></param>
        public MapSubEditorWindow(ref GameObject empty, List<GameObject> parts, Vector2 map)
        {
            saveObject = empty;
            partsObject = parts;
            mapSize = map;

            //セルの二次元配列を準備
            cell = new GridCell[(int)map.x, (int)map.y];
            pallet = new Pallet(partsObject);
        }

        /// <summary>
        /// マウスイベント
        /// </summary>
        private void MouseEvent()
        {
            Event e = Event.current;
            if(e.type == EventType.MouseDown)
            {
                //マウスをクリックした座標入力
                Vector2 clickPos = Event.current.mousePosition;

                int searchCell_X = (int)(clickPos.x / gridSize);

                int searchCell_Y = (int)(clickPos.y / gridSize);
                //マップ外をクリックされたら、返す
                if (searchCell_X >= mapSize.x || searchCell_Y >= mapSize.y) return;

                Debug.Log("X:" + searchCell_X + "   Y:" + searchCell_Y);
            }
        }

        /// <summary>
        /// グリッド線を描画
        /// </summary>
        /// <param name="grid"></param>
        private void DrawGrid(Vector2 grid)
        {
            //色を黒にする
            Handles.color = gridColor;

            for (int yyy =0; yyy < grid.y; yyy++)
            {
                for(int xxx = 0; xxx < grid.x; xxx++)
                {
                    Handles.DrawLine(new Vector2(xxx, yyy) * gridSize, new Vector2(xxx + 1, yyy) * gridSize);
                    Handles.DrawLine(new Vector2(xxx, yyy) * gridSize, new Vector2(xxx, yyy + 1) * gridSize);
                }
            }

            Handles.DrawLine(new Vector2(grid.x, 0) * gridSize, new Vector2(grid.x, grid.y) * gridSize);
            Handles.DrawLine(new Vector2(0, grid.y) * gridSize, new Vector2(grid.x, grid.y) * gridSize);
        }

        public void OnGUI()
        {
            //GridとPalletを横に並べて表示する
            using (new GUILayout.HorizontalScope())
            {
                #region ### Grid ###
                //色の変更
                GUI.color = backGroundColor;

                //横に並べる
                using (new GUILayout.HorizontalScope(GUILayout.Width((Screen.width / 4) * 3)))
                {
                    using (new GUILayout.ScrollViewScope(scrollPosition, GUI.skin.box))
                    {
                        DrawGrid(mapSize);
                    }
                }
                GUI.color = Color.white;
                #endregion

                #region ### Pallet ###
                //画面の４分の１から、バーのサイズ(8px)分引いている
                using (var scrollView = new GUILayout.ScrollViewScope(scrollPosition, GUILayout.Width((Screen.width / 4) - 8)))
                {
                    scrollPosition = scrollView.scrollPosition;
                    pallet.ToggleView();
                }
                #endregion
            }

            #region ### Tabs ###
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {

                if (GUILayout.Button("Export", EditorStyles.toolbarButton, GUILayout.Width(70))) //! 出力
                {

                }
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(70))) //! クリア
                {

                }
                if (GUILayout.Button("Option", EditorStyles.toolbarButton, GUILayout.Width(70))) //! 設定
                {

                }
                if (GUILayout.Button("Origin", EditorStyles.toolbarButton, GUILayout.Width(70))) //! 原点
                {

                }
                if (GUILayout.Button("Paint", EditorStyles.toolbarButton, GUILayout.Width(70))) //! ペイント
                {

                }
                if (GUILayout.Button("Eraser", EditorStyles.toolbarButton, GUILayout.Width(70))) //! 消しゴム
                {

                }
                if (GUILayout.Button("bucket", EditorStyles.toolbarButton, GUILayout.Width(70))) //! バケツ
                {

                }

                //グリッドサイズの変更
                gridSize = EditorGUILayout.Slider(gridSize / 10, 1, 10) * 10;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(1);
            #endregion

            MouseEvent();
        }

    }

    /// <summary>
    /// パレットクラス
    /// @author Shoichi Ikeda
    /// </summary>
    public class Pallet
    {
        public List<GameObject> paint; //! 使用するデータ群
        public GameObject usePaint;  //! 現在使用しているデータ
        bool[] radioButton; //! ボタン

        /// <summary>
        /// パレットに使用するデータ群を読み込む
        /// </summary>
        /// <param name="objects"></param>
        public Pallet(List<GameObject> objects)
        {
            paint = objects;
            radioButton = new bool[objects.Count];
        }

        /// <summary>
        /// トグルを表示する
        /// </summary>
        public void ToggleView()
        {
            using (new GUILayout.VerticalScope())
            {
                for (int i = 0; i < paint.Count; i++)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        radioButton[i] = EditorGUILayout.Toggle(paint[i] == usePaint, GUILayout.Width(10));
                        //ボタンが押されているなら、使用するオブジェクトとして保管する
                        if (radioButton[i]) usePaint = paint[i];
                        GUILayout.Label(paint[i].name);
                    }
                }
            }
        }
    }

    /// <summary>
    /// グリッド毎のデータ
    /// @author Shoichi Ikeda
    /// </summary>
    struct GridCell
    {
        #region ### Global ###
        public GameObject cellObject; //! セルのオブジェクト
        public Texture cellTexture; //! セルに描画するテクスチャ
        #endregion

        #region ### Event ###
        /// <summary>
        /// イベント情報を入力して、その情報と同様の処理を行う
        /// </summary>
        public void InputEvent(MouseEvents inputEvent)
        {
            switch(inputEvent)
            {
                case MouseEvents.paint:
                    CellPaint();
                    break;

                case MouseEvents.eraser:
                    CellEraser();
                    break;
            }
        }

        /// <summary>
        /// セルを塗る
        /// </summary>
        private void CellPaint()
        {
            Debug.Log("Cell Paint!!!");
        }

        /// <summary>
        /// セルを初期化
        /// </summary>
        private void CellEraser()
        {
            Debug.Log("Cell Eraser!!!");
        }

        #endregion
    }

    /// <summary>
    /// マウスで操作するときに使用するイベント
    /// @author Shoichi Ikeda
    /// </summary>
    enum MouseEvents
    {
        none,
        paint,
        eraser
    }
}
