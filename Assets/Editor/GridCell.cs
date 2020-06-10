using UnityEditor;
using UnityEngine;

namespace MapEditor
{
    /// <summary>
    /// セルのデータ群
    /// @author Shoichi Ikeda
    /// </summary>
    public struct GridCell
    {
        public GameObject cellObject;    //! セルのオブジェクト
        public Color cellColor;          //! セルの色
        Vector2 pos;                     //! 座標
        Vector2 mapSize;                 //! マップのサイズ
        public int parentNumber;         //! 親ナンバー


        /// <summary>
        /// イベント情報を入力して、その情報と同様の処理を行う
        /// </summary>
        public void InputEvent(MouseEvents inputEvent, Color color, GameObject inputObject)
        {
            switch (inputEvent)
            {
                case MouseEvents.paint:
                    cellColor = color;
                    cellObject = inputObject;
                    CellPaint();
                    break;

                case MouseEvents.eraser:
                    CellEraser();
                    break;
            }
        }

        /// <summary>
        /// セルの初期化
        /// </summary>
        public void InitCell(Vector2 inputPos, Vector2 inputSize, Color inputColor)
        {
            pos = inputPos;
            mapSize = inputSize;
            cellColor = inputColor;
            parentNumber = 0;
        }

        /// <summary>
        /// サイズの変更
        /// </summary>
        /// <param name="reSize"></param>
        public void Resize(float reSize)
        {
            mapSize = new Vector2(reSize, reSize);
        }

        /// <summary>
        /// セルを塗る
        /// </summary>
        public void CellPaint()
        {
            //色の描画
            EditorGUI.DrawRect(new Rect(pos * mapSize, mapSize), cellColor);
        }

        /// <summary>
        /// セルを初期化
        /// </summary>
        private void CellEraser()
        {
            cellColor = Color.white;
            cellObject = null;
            //色を初期化
            EditorGUI.DrawRect(new Rect(pos * mapSize, mapSize), cellColor);
        }

    }

    /// <summary>
    /// マウスで操作するときに使用するイベント
    /// @author Shoichi Ikeda
    /// </summary>
    public enum MouseEvents
    {
        none,
        paint,
        eraser
    }
}