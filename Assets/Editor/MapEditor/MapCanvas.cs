using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

namespace MapEditor
{
    public class MapCanvas : EditorWindow
    {
        Vector2 gridScrollPosition = new Vector2(0, 0);  //! グリッドスクロール
        Vector2 palletScrollPosition = new Vector2(0, 0);//! パレットスクロール
        Vector2 clickPos = new Vector2(0, 0);            //! クリックした座標

        Event mouseEvent;                                //! マウスのイベント
        MouseEvents selectEvent = MouseEvents.none;      //! 選択されているイベント

        Color gridColor = Color.gray;                    //! 線の色
        Color backGroundColor = Color.white;             //! 背景色

        GameObject saveObject;                           //! 保存するオブジェクト
        List<GameObject> partsObjects;                   //! パーツの保管

        GridCell[,] cell = new GridCell[10, 10];         //! セルデータ
		int parentNumber = 0;                        //! 入力する親番号
        float canvasSize = 1.5f;
        float gridSize = 30;                             //! グリッドのサイズ
        Vector2 mapSize;                                 //! マップのサイズ

        Pallet pallet;                                   //! パレット

		bool paint = false;
		bool eraser = false;
        bool bucket = false;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="emptyObject"></param>
		/// <param name="stageParts"></param>
		/// <param name="mapSize"></param>
		public MapCanvas(GameObject emptyObject, List<GameObject> stageParts, Vector2 mapSize)
        {
            saveObject = emptyObject;
            partsObjects = stageParts;

            pallet = new Pallet(partsObjects);

            ReMapSize(mapSize);
        }

        /// <summary>
        /// マップのサイズを変更する
        /// </summary>
        public void ReMapSize(Vector2 newMapSize)
        {
            //マップのサイズを置き換える
            mapSize = newMapSize;

            //セルのサイズを変更する
            cell = (GridCell[,])ArrayReSize(cell, new int[]{ (int)mapSize.x, (int)mapSize.y });


            //初期化
            for(int yyy = 0; yyy < mapSize.y; yyy++)
            {
                for (int xxx = 0; xxx < mapSize.x; xxx++)
                {
                    if (cell[xxx, yyy].cellObject == null)
                    {
                        //セルの初期化
                        cell[xxx, yyy].InitCell(new Vector2(xxx, yyy), new Vector2(gridSize, gridSize), backGroundColor);
                    }
                }
            }
        }

        /// <summary>
        /// マウスの左を押している場合の処理
        /// </summary>
        private void MouseDownPaint()
        {
            int searchCell_X = (int)(clickPos.x / gridSize);
            int searchCell_Y = (int)(clickPos.y / gridSize);

            //マップ外をクリックされたら、返す
            if (searchCell_X >= mapSize.x || searchCell_Y >= mapSize.y) return;

            cell[searchCell_X, searchCell_Y].InputEvent(selectEvent, pallet.GetColor(pallet.GetPaintValue()), pallet.paint[pallet.GetPaintValue()]);
        }

		/// <summary>
		/// マウスの右を押している場合の処理
		/// </summary>
		private void MouseDownParent()
		{
			int searchCell_X = (int)(clickPos.x / gridSize);
			int searchCell_Y = (int)(clickPos.y / gridSize);

			//マップ外をクリックされたら、返す
			if (searchCell_X >= mapSize.x || searchCell_Y >= mapSize.y) return;

			cell[searchCell_X, searchCell_Y].SetParentNumber(parentNumber);
		}

        /// <summary>
        /// グリッドを描く
        /// </summary>
        /// <param name="grid"></param>
        public void DrawGrid(Vector2 grid)
        {
            //線の色
            Handles.color = gridColor;

            for (int yyy = 0; yyy < grid.y; yyy++)
            {
                for (int xxx = 0; xxx < grid.x; xxx++)
                {
                    cell[xxx, yyy].CellPaint();
                    cell[xxx, yyy].Resize(gridSize);

                    Handles.DrawLine(new Vector2(xxx, yyy) * gridSize, new Vector2(xxx + 1, yyy) * gridSize);
                    Handles.DrawLine(new Vector2(xxx, yyy) * gridSize, new Vector2(xxx, yyy + 1) * gridSize);
                    
                }

                Handles.DrawLine(new Vector2(grid.x, 0) * gridSize, new Vector2(grid.x, grid.y) * gridSize);
                Handles.DrawLine(new Vector2(0, grid.y) * gridSize, new Vector2(grid.x, grid.y) * gridSize);
            }
        }

        /// <summary>
        /// 出力する
        /// </summary>
        private void Export()
        {
            for (int yyy = 0; yyy < mapSize.y; yyy++)
            {
                for (int xxx = 0; xxx < mapSize.x; xxx++)
                {
                    //オブジェクトデータがあるなら、配置
                    if (cell[xxx, yyy].cellObject)
                    {
                        Vector3 cellPos = new Vector3
                            (cell[xxx, yyy].cellObject.transform.localScale.x * xxx,
                             cell[xxx, yyy].cellObject.transform.localScale.y,
                             cell[xxx, yyy].cellObject.transform.localScale.z * yyy);


                        GameObject obj = Instantiate(cell[xxx, yyy].cellObject,
                                                     cellPos,
                                                     cell[xxx, yyy].cellObject.transform.rotation);


                        obj.transform.parent = saveObject.transform;
                    }
                }
            }

            Debug.Log("Finish Exporting!");
        }

        /// <summary>
        /// 描画
        /// </summary>
        private void Draw()
        {
            using (new GUILayout.HorizontalScope(GUILayout.Width(Screen.width)))
            {
                //色の変更
                GUI.color = backGroundColor;

                //横に並べる
                using (new GUILayout.HorizontalScope(GUILayout.Width((Screen.width / 4) * canvasSize)))
                {
                    using (new GUILayout.ScrollViewScope(gridScrollPosition, GUI.skin.box))
                    {
                        DrawGrid(mapSize);
                    }
                }
                GUI.color = Color.white;

                //画面の４分の１から、バーのサイズ(8px)分引いている
                using (var scrollView = new GUILayout.ScrollViewScope(palletScrollPosition, GUILayout.Width((Screen.width / 4) - 8)))
                {
                    palletScrollPosition = scrollView.scrollPosition;
                    pallet.ToggleView();
                }
            }
        }

        /// <summary>
        /// タブの表示
        /// </summary>
        private void ToolBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                //ボタンの準備
                GUIContent paintToggle = new GUIContent("Paint");
                GUIContent eraserToggle = new GUIContent("Eraser");
                GUIContent bucketToggle = new GUIContent("Bucket");


                //書き出し
                if (GUILayout.Button("Export", EditorStyles.toolbarButton, GUILayout.Width(70))) //! 出力
                {
                    Export();
                }

                //塗り
                paint = GUILayout.Toggle(paint, paintToggle, EditorStyles.toolbarButton, GUILayout.Width(70));
                if (paint)
                {
                    selectEvent = MouseEvents.paint;
                    eraser = false;
                    bucket = false;
                }

                //消しゴム
                eraser = GUILayout.Toggle(eraser, eraserToggle, EditorStyles.toolbarButton, GUILayout.Width(70));
                if (eraser)
                {
                    selectEvent = MouseEvents.eraser;
                    paint = false;
                    bucket = false;
                }

                //バケツ
                bucket = GUILayout.Toggle(bucket, bucketToggle, EditorStyles.toolbarButton, GUILayout.Width(70));               
                if (bucket)
                {
                    selectEvent = MouseEvents.bucket;
                    paint = false;
                    eraser = false;
                }
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                GUILayout.Label("Grid Size");
				//グリッドサイズの変更
				gridSize = EditorGUILayout.Slider((gridSize / 10), 1, 10, GUILayout.Width(150)) * 10;
				GUISupport.DrawLineVertical();


                GUILayout.Label("Parent Number");
                //親番号
                parentNumber = (int)EditorGUILayout.Slider(parentNumber, 0, 100, GUILayout.Width(150));

        
                GUILayout.Label("Pallet Size");
                //パレットのサイズ
                canvasSize = EditorGUILayout.Slider(canvasSize, 1, 10, GUILayout.Width(150));
			}
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(1);
        }

        /// <summary>
        /// マウスを押すことによるイベント
        /// </summary>
        private void MouseEvent()
        {
            mouseEvent = Event.current;

            clickPos = Event.current.mousePosition;

			switch (mouseEvent.type)
			{
				case EventType.MouseDown:
					if (Event.current.shift)	//! シフトキーが押されている
						MouseDownParent();
					else
						MouseDownPaint();
					break;
			}
		}

		private void OnGUI()
        {
            MouseEvent();

            Draw();

            ToolBar();
        }

        /// <summary>
        /// 多次元配列のサイズを変更する
        /// [参照]
        /// https://docs.microsoft.com/ja-jp/dotnet/api/system.array.resize?view=netcore-3.1
        /// </summary>
        /// <param name="array"></param>
        /// <param name="newSizes"></param>
        /// <returns></returns>
        public Array ArrayReSize(Array array, int[] newSizes)
        {
            //次元同士が一致しなければエラー
            if (newSizes.Length != array.Rank)
            {
                throw new ArgumentException("Array Error");
            }

            var temp = Array.CreateInstance(array.GetType().GetElementType(), newSizes);
            int length = array.Length <= temp.Length ? array.Length : temp.Length;
            Array.ConstrainedCopy(array, 0, temp, 0, length);
            return temp;
        }
    }

}