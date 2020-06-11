using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapEditor
{
    /// <summary>
    /// バケツツール
    /// @author Shoichi Ikeda
    /// </summary>
    public class Bucket : MonoBehaviour
    {
        GridCell[,] cells;       //! セルデータ
        GameObject searchObject; //! 探索対象
        GameObject drawObject;   //! 塗るオブジェクト
        Color drawColor;         //! 塗る色

        //コンストラクタ
        public Bucket(GridCell[,] inputCells, GameObject searchObject, GameObject drawObject, Color color)
        {
            this.cells = inputCells;
            this.searchObject = searchObject;
            this.drawObject = drawObject;
            this.drawColor = color;
        }

        public void Search(int x, int y)
        {
            cells[x, y].InputEvent(MouseEvents.paint, drawColor, drawObject);

            //Top
            if(HasObject(x, y - 1))
            {
                Search(x, y - 1);
            }

            //Left
            if(HasObject(x - 1, y))
            {
                Search(x - 1, y);
            }

            //Right
            if(HasObject(x + 1, y))
            {
                Search(x + 1, y);
            }

            //Bottom
            if(HasObject(x, y + 1))
            {
                Search(x, y + 1);
            }
        }

        /// <summary>
        /// オブジェクトを持っているかを判定する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool HasObject(int x, int y)
        {
            //0以下になったら、処理を中断
            if (x < 0 || y < 0) return false;
            if (x >= cells.GetLength(0) || y >= cells.GetLength(1)) return false;

            //塗るオブジェクトがNullの場合
            if(searchObject == null)
            {
                return cells[x, y].cellObject == null;
            }

            //塗るオブジェクト同士が等しい
            return searchObject.Equals(cells[x, y].cellObject);
        }
    }
}
