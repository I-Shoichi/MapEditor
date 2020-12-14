using UnityEngine;

namespace Colouring
{
    /// <summary>
    /// 色を設定する(最大:144)
    /// 協力者:友人K
    /// </summary>
    public static class ColorSetter
    {
        static int hMax = 3;       // 色相（hue）の最大分割量
        static int sMax = 4;       // 彩度（sarturation）の最大分割量
        static int vMax = 3;       // 明度（value）の最大分割量

        public static Color GetColor(int value)
        {
            int hueAngle = 360 / hMax;
            float h, s, v;

            h = hueAngle * (value % hMax);
            h += hueAngle / (value / hMax + 1) % hueAngle;
            h = (h % 360) / 360f;

            s = (value / sMax) % sMax;
            s /= sMax;

            v = (value / vMax) % vMax;
            v /= vMax;

            Color col = Color.HSVToRGB(h, 1 - s, 1 - v);
            return col;
        }
    }
}
