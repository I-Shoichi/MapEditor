using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MapEditor
{
    /// <summary>
    /// パレットクラス
    /// @author Shoichi Ikeda
    /// </summary>
    public class Pallet : MonoBehaviour
    {
        public List<GameObject> paint; 　　　//! 使用するデータ群
        public GameObject usePaint;         //! 現在使用しているデータ
        bool[] radioButton;                 //! ボタン
        Color[] colors;                     //! 描画する色

        /// <summary>
        /// 使用しているオブジェクトのValueを取得
        /// </summary>
        /// <returns></returns>
        public int GetPaintValue()
        {
            return paint.IndexOf(usePaint);
        }

        /// <summary>
        /// 色を取得
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Color GetColor(int value)
        {
            return colors[value];
        }

        /// <summary>
        /// 色を配置
        /// </summary>
        /// <param name="value"></param>
        /// <param name="color"></param>
        public void SetColor(int value, Color color)
        {
            colors[value] = color;
        }

        /// <summary>
        /// パレットに使用するデータ群を読み込む
        /// </summary>
        /// <param name="objects"></param>
        public Pallet(List<GameObject> objects)
        {
            paint = objects;
            radioButton = new bool[objects.Count];
            colors = new Color[objects.Count];
            SetRandomColor();
            radioButton[0] = true;
            usePaint = paint[0];
        }

        /// <summary>
        /// ランダムで色を生成
        /// </summary>
        public void SetRandomColor()
        {
            for (int i = 0; i < colors.Length; i++)
            {
                //色をランダムで配置
                colors[i] = Color.HSVToRGB(Random.RandomRange(0, 100) * 0.01f, Random.RandomRange(0, 100) * 0.01f, Random.RandomRange(0, 100) * 0.01f);
            }
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
                        GUILayout.Label(paint[i].name, GUILayout.Width(150));

                        //色の変更フィールドを配置
                        colors[i] = EditorGUILayout.ColorField(colors[i], GUILayout.Width(80));
                    }
                }
            }
        }
    }
    
}
