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
        /*ユーザーの初期設定*/
        Object dataDirectory;  //! 使用するオブジェクトが入っているディレクトリ
        Object outputEmptyObject; //! 作成したマップデータを保管するオブジェクト
        Vector2 mapSize = new Vector2(10, 10); //! マップサイズを保管
        Vector3 partsSize = new Vector3(1, 1, 1); //!使用するオブジェクトのサイズを予め記述し、サイズの成型を行う

        /*Developer Settings*/
        const string WINDOW_NAME = "Map Editor"; //! タブに表示される名前
        const int TITLE_FONT_SIZE = 32; //! タイトルに使用するフォントサイズ
        const int HEADER_FONT_SIZE = 20; //! ヘッダーに使用するフォントサイズ

        [MenuItem("Window/"+WINDOW_NAME)]
        private static void Create()
        {
            GetWindow<MapEditorWindow>(WINDOW_NAME);
        }

        private void OnGUI()
        {

        }
    }
}
