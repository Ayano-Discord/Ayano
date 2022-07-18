using System;
using System.Collections.Generic;
using System.Drawing;
using Remora.Discord;

namespace Ayano.Core.Services.Utilities
{
    public class ColorUtils
    {
        /// <summary>
        /// Returns rainbow table of <see cref="colorCount"/> colors
        /// </summary>
        public static List<Color> GetRainbowColors(int colorCount)
        {
            var ret = new List<Color>(colorCount);
            var p = 360f / colorCount;

            for (var n = 0; n < colorCount; n++)
            {
                ret.Add(HsvToRgb(n * p, 1f, 1f));
            }

            return ret;
        }

        /// <summary>
        /// HSV -> RGB color
        /// </summary>
        public static Color HsvToRgb(float h, float s, float v)
        {
            var hi = (int) Math.Floor(h / 60.0) % 6;
            var f = (h / 60f) - MathF.Floor(h / 60f);

            var p = (int)(v * (1f - s));
            var q = (int)(v * (1f - f * s));
            var t = (int)(v * (1f - (1f - f) * s));

            var color = hi switch
            {
                0 => Color.FromArgb(1, (int)v, t, p),
                1 => Color.FromArgb(1, q, (int)v, p),
                2 => Color.FromArgb(1, p, (int)v, t),
                3 => Color.FromArgb(1, p, q, (int)v),
                4 => Color.FromArgb(1, t, p, (int)v),
                5 => Color.FromArgb(1, (int)v, p, q),
                _ => Color.FromArgb(1, 0x00, 0x00, 0x00)
            };
            return color;
        }
    }
}
