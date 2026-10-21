using System;
using UnityEngine;

namespace JahNomad.Reutov
{
    /// <summary>Original procedural sprites for the Yubileyny courtyard and touch game interface.</summary>
    public static partial class ReutovRuntimeArt
    {
        private static void MakePanel(string name, string message, int scale)
        {
            int width = name.StartsWith("Battery") ? 350 : 1120;
            var p = new PixelCanvas(width, 104, C(8, 18, 32, 242));
            p.Rect(4, 4, width - 8, 96, C(7, 25, 39, 243));
            p.Rect(4, 4, width - 8, 3, C(56, 216, 223));
            p.Rect(4, 97, width - 8, 3, C(61, 168, 185));
            p.Rect(4, 4, 3, 96, C(56, 216, 223));
            p.Rect(width - 7, 4, 3, 96, C(56, 216, 223));
            p.TextCentered(message, scale, C(243, 228, 200), 50);
            p.Save(name);
        }

        private static void MakeControl(string name, int type)
        {
            var p = new PixelCanvas(160, 160, new Color32(0, 0, 0, 0));
            if (type == 0)
            {
                p.Circle(80, 80, 76, C(94, 139, 157, 151));
                p.Circle(80, 80, 72, C(10, 29, 46, 206));
                p.Circle(80, 80, 57, C(33, 56, 75, 160));
                p.Circle(80, 80, 51, C(12, 33, 50, 210));
            }
            else if (type == 1)
            {
                p.Circle(80, 80, 74, C(124, 197, 206));
                p.Circle(80, 80, 65, C(36, 85, 110));
                p.Circle(63, 62, 23, C(147, 204, 208, 190));
            }
            else
            {
                p.Circle(80, 80, 71, C(74, 171, 193, 203));
                p.Circle(80, 80, 65, C(6, 31, 51, 232));
                p.TextCentered(type == 2 ? "БЕГ" : type == 3 ? "ДЕЙСТВИЕ" : "СВЕТ",
                    type == 3 ? 2 : 3, C(245, 226, 192), 80);
            }
            p.Save(name);
        }

        private static void MakeBeam(string name, Color32 tint)
        {
            var p = new PixelCanvas(220, 180, new Color32(0, 0, 0, 0));
            for (int row = 0; row < 175; row++)
            {
                int half = (175 - row) * 90 / 175;
                byte alpha = (byte)Mathf.Clamp(tint.a * (175 - row) / 175, 0, 255);
                p.Rect(110 - half, row, half * 2, 1,
                    C(tint.r, tint.g, tint.b, alpha));
            }
            p.Save(name);
        }

        private static void MakePerson(string name, int kind)
        {
            var p = new PixelCanvas(48, 80, new Color32(0, 0, 0, 0));
            Color32 jacket = kind == 0 ? C(47, 74, 113) :
                kind == 1 ? C(62, 44, 62) : C(16, 29, 43);
            Color32 trousers = kind == 0 ? C(51, 73, 106) :
                kind == 1 ? C(31, 39, 47) : C(20, 26, 33);
            Color32 hair = kind == 1 ? C(82, 36, 51) : C(51, 34, 32);
            p.Rect(12, 54, 10, 19, trousers);
            p.Rect(26, 54, 10, 19, trousers);
            p.Rect(10, 72, 13, 5, C(179, 184, 171));
            p.Rect(25, 72, 13, 5, C(179, 184, 171));
            p.Rect(7, 34, 34, 25, C(18, 25, 33));
            p.Rect(11, 33, 26, 26, jacket);
            p.Rect(7, 37, 7, 20, jacket);
            p.Rect(34, 37, 7, 20, jacket);
            if (kind == 0)
            {
                p.Rect(23, 34, 3, 25, C(214, 199, 185));
                p.Rect(11, 43, 25, 3, C(168, 80, 79));
                p.Rect(33, 49, 5, 9, C(187, 117, 76));
            }
            if (kind == 1)
            {
                p.Rect(11, 35, 4, 23, C(124, 52, 73));
                p.Rect(23, 38, 5, 18, C(152, 64, 89));
            }
            p.Rect(10, 10, 29, 26, hair);
            p.Rect(14, 18, 21, 20, C(204, 152, 118));
            p.Rect(17, 23, 4, 3, C(31, 30, 40));
            p.Rect(29, 23, 4, 3, C(31, 30, 40));
            p.Rect(23, 31, 7, 2, C(130, 72, 72));
            p.Rect(10, 11, 29, 8, hair);
            if (kind == 1)
            {
                p.Rect(8, 17, 7, 32, hair);
                p.Rect(36, 17, 7, 29, hair);
            }
            if (kind == 2)
            {
                p.Rect(7, 13, 35, 8, C(9, 17, 27));
                p.Rect(15, 10, 20, 6, C(19, 33, 46));
                p.Rect(15, 24, 20, 8, C(43, 43, 46));
            }
            p.Save(name);
        }

        private static PixelCanvas MakeCourtyard()
        {
            var p = new PixelCanvas(640, 360, C(8, 17, 36));
            var rng = new System.Random(1996);
            for (int i = 0; i < 250; i++)
            {
                int x = rng.Next(640), y = rng.Next(74);
                p.Rect(x, y, i % 8 == 0 ? 2 : 1, 1,
                    i % 6 == 0 ? C(191, 221, 234) : C(74, 134, 160));
            }
            p.Circle(438, 40, 21, C(133, 155, 171));
            p.Circle(434, 37, 17, C(212, 220, 213));
            Block(p, 0, 51, 173, 131, 8, 8, rng);
            Block(p, 184, 72, 129, 110, 6, 7, rng);
            Block(p, 332, 49, 124, 138, 6, 8, rng);
            Block(p, 477, 67, 163, 117, 8, 7, rng);
            p.Rect(0, 181, 640, 179, C(29, 41, 52));
            for (int i = 0; i < 630; i += 26)
            {
                p.Rect(i, 184 + ((i * 7) % 20), 11, 2, C(43, 55, 62));
                p.Rect(i * 17 % 620, 224 + (i * 13) % 130, 13, 2, C(39, 65, 77));
            }
            // Kiosk is drawn as a real street object, with its sign ON its own fascia.
            p.Rect(472, 83, 99, 63, C(45, 54, 65));
            p.Rect(472, 86, 99, 7, C(177, 78, 54));
            p.Rect(479, 93, 85, 17, C(89, 38, 39));
            p.TextAt("ПРОДУКТЫ", 2, C(254, 196, 115), 483, 95);
            p.Rect(478, 115, 86, 23, C(247, 178, 95));
            p.Rect(481, 118, 80, 16, C(65, 57, 52));
            for (int j = 0; j < 26; j++)
                p.Rect(483 + (j % 13) * 6, 119 + (j / 13) * 9, 4, 6,
                    j % 2 == 0 ? C(190, 115, 78) : C(153, 173, 114));
            // Garage, diary and paranormal trace.
            p.Rect(8, 235, 105, 53, C(45, 55, 63));
            p.Rect(15, 240, 92, 47, C(75, 83, 90));
            p.Rect(18, 245, 88, 3, C(39, 43, 50));
            p.Rect(130, 303, 18, 9, C(229, 204, 154));
            p.Rect(128, 312, 32, 7, C(12, 23, 36));
            for (int i = 0; i < 20; i++)
                p.Rect(125 + rng.Next(45), 306 + rng.Next(23), 3, 1, C(44, 77, 99));
            // Rusted Moskvich with an opened bonnet.
            p.Rect(51, 161, 138, 70, C(22, 30, 35));
            p.Rect(55, 168, 132, 53, C(107, 50, 42));
            p.Rect(70, 172, 45, 30, C(45, 67, 76));
            p.Rect(122, 170, 53, 31, C(70, 46, 45));
            p.Rect(180, 181, 24, 9, C(139, 70, 52));
            p.Rect(61, 210, 25, 15, C(25, 26, 32));
            p.Rect(159, 210, 23, 15, C(25, 26, 32));
            p.Rect(178, 178, 2, 38, C(232, 134, 72));
            p.Rect(91, 168, 24, 4, C(177, 84, 62));
            // A black Volga belongs to the laboratory patrols.
            p.Rect(424, 154, 138, 54, C(13, 20, 31));
            p.Rect(432, 160, 119, 32, C(27, 39, 51));
            p.Rect(446, 163, 45, 17, C(56, 83, 92));
            p.Rect(504, 163, 39, 17, C(37, 66, 78));
            p.Rect(443, 195, 25, 12, C(11, 15, 24));
            p.Rect(523, 195, 25, 12, C(11, 15, 24));
            // Static swings, sandbox, bench and concrete planter.
            p.Line(462, 250, 474, 294, C(166, 72, 57));
            p.Line(548, 250, 537, 294, C(166, 72, 57));
            p.Line(462, 250, 548, 250, C(176, 82, 54));
            p.Line(489, 251, 489, 283, C(94, 136, 143));
            p.Line(520, 251, 520, 283, C(94, 136, 143));
            p.Rect(484, 284, 13, 3, C(159, 82, 57));
            p.Rect(515, 284, 13, 3, C(159, 82, 57));
            p.Rect(525, 295, 70, 42, C(103, 85, 61));
            p.Rect(532, 299, 58, 32, C(138, 114, 69));
            p.Rect(215, 292, 35, 29, C(80, 89, 91));
            p.Rect(347, 287, 48, 23, C(55, 57, 59));
            for (int i = 0; i < 130; i++)
            {
                int x = rng.Next(640), y = rng.Next(198, 357);
                p.Rect(x, y, rng.Next(2, 14), 1,
                    i % 3 == 0 ? C(109, 77, 67) : C(35, 72, 90));
            }
            foreach (int x in new []{150, 391})
            {
                p.Rect(x, 154, 3, 93, C(80, 83, 76));
                p.Rect(x - 4, 150, 12, 6, C(252, 190, 99));
                for (int j = 0; j < 13; j++)
                    p.Rect(x - 22 + j * 3, 246 + j / 3, 4, 1, C(119, 78, 56));
            }
            for (int i = 0; i < 38; i++)
            {
                int x = rng.Next(640), y = rng.Next(181, 220);
                p.Circle(x, y, rng.Next(5, 11),
                    i % 2 == 0 ? C(21, 49, 51) : C(25, 45, 54));
            }
            return p;
        }

        private sealed partial class PixelCanvas
        {
            public void Line(int x0, int y0, int x1, int y1, Color32 color)
            {
                int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
                int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
                int error = dx + dy;
                while (true)
                {
                    Rect(x0, y0, 2, 2, color);
                    if (x0 == x1 && y0 == y1) break;
                    int twice = error * 2;
                    if (twice >= dy) { error += dy; x0 += sx; }
                    if (twice <= dx) { error += dx; y0 += sy; }
                }
            }

            public void TextAt(string text, int scale, Color32 color, int left, int top)
            {
                for (int index = 0; index < text.Length; index++)
                {
                    string[] glyph;
                    if (!Glyph.TryGetValue(char.ToUpperInvariant(text[index]), out glyph)) continue;
                    for (int row = 0; row < 7; row++)
                        for (int col = 0; col < 5; col++)
                            if (glyph[row][col] == '1')
                                Rect(left + index * 6 * scale + col * scale,
                                     top + row * scale, scale, scale, color);
                }
            }
        }
    }
}
