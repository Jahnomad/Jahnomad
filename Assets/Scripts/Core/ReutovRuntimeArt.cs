using System;
using System.Collections.Generic;
using UnityEngine;

namespace JahNomad.Reutov
{
    /// <summary>Self-contained graphic fallback. No asset imports, network or editor APIs required.</summary>
    public static partial class ReutovRuntimeArt
    {
        private static readonly Dictionary<string, Texture2D> Images = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        public static Sprite GetSprite(string name)
        {
            if (Sprites.TryGetValue(name, out Sprite existing)) return existing;
            if (!Images.ContainsKey(name))
            {
                switch (name)
                {
                    case "Backdrop": MakeBackground().Save(name); break;
                    case "StudioLogo": MakeBanner("JAH NOMAD", "НЕЗАВИСИМАЯ ИГРОВАЯ СТУДИЯ", name, new Color32(230, 103, 91, 255)); break;
                    case "GameTitle": MakeBanner("ЗАТЕРЯННЫЕ", "В РЕУТОВЕ", name, new Color32(242, 180, 109, 255)); break;
                    case "NewGame": MakeButton(name, "НОВАЯ ИГРА", false); break;
                    case "Continue": MakeButton(name, "ВО ДВОР", false); break;
                    case "CourtyardBackdrop": MakeCourtyard().Save(name); break;
                    case "Misha": MakePerson(name, 0); break;
                    case "Regina": MakePerson(name, 1); break;
                    case "Agent": MakePerson(name, 2); break;
                    case "PatrolCone": MakeBeam(name, C(240, 225, 168, 42)); break;
                    case "FlashCone": MakeBeam(name, C(251, 220, 160, 52)); break;
                    case "ChapterTitle": MakePanel(name, "ГЛАВА 01 - КОМЕНДАНТСКИЙ ЧАС", 3); break;
                    case "ObjectiveBattery": MakePanel(name, "НАЙДИТЕ БАТАРЕЙКИ В МОСКВИЧЕ", 3); break;
                    case "ObjectiveDiary": MakePanel(name, "НАЙДИТЕ ДНЕВНИК У ГАРАЖЕЙ", 3); break;
                    case "ObjectiveRegina": MakePanel(name, "ПОГОВОРИТЕ С РЕГИНОЙ", 3); break;
                    case "ObjectiveComplete": MakePanel(name, "РЕГИНА: БРАТ ПРОПАЛ. НАДО ИСКАТЬ!", 3); break;
                    case "Battery0": MakePanel(name, "БАТАРЕЙКИ 0/2", 2); break;
                    case "Battery2": MakePanel(name, "БАТАРЕЙКИ 2/2", 2); break;
                    case "QuestNotice": MakePanel(name, "ОСМОТРИТЕ ДВОР", 4); break;
                    case "Detected": MakePanel(name, "ВАС ЗАМЕТИЛИ! ВЕРНИТЕСЬ В УКРЫТИЕ", 3); break;
                    case "FoundBattery": MakePanel(name, "В МОСКВИЧЕ НАЙДЕНЫ 2 БАТАРЕЙКИ!", 3); break;
                    case "FoundDiary": MakePanel(name, "ДНЕВНИК БРАТА И ЧЁРНАЯ СЛИЗЬ...", 3); break;
                    case "ReginaDialogue": MakePanel(name, "РЕГИНА: МИША, БЕЖИМ В ПОДЪЕЗД!", 3); break;
                    case "GetCloser": MakePanel(name, "ПОДОЙДИТЕ БЛИЖЕ И НАЖМИТЕ ДЕЙСТВИЕ", 3); break;
                    case "JoyBase": MakeControl(name, 0); break;
                    case "JoyKnob": MakeControl(name, 1); break;
                    case "Run": MakeControl(name, 2); break;
                    case "Action": MakeControl(name, 3); break;
                    case "Light": MakeControl(name, 4); break;
                    case "Settings": MakeButton(name, "НАСТРОЙКИ", false); break;
                    case "Quit": MakeButton(name, "ВЫХОД", false); break;
                    case "Back": MakeButton(name, "НАЗАД", true); break;
                    case "Skip": MakeButton(name, "ПРОПУСТИТЬ", true); break;
                    case "SoundOn": MakeButton(name, "ЗВУК: ВКЛ", true); break;
                    case "SoundOff": MakeButton(name, "ЗВУК: ВЫКЛ", true); break;
                    case "PrologueCard": MakeCard(name, "РЕУТОВ. 1996 ГОД.", "ПОСЛЕ ПОЛУНОЧИ ГОРОД НЕ УЗНАТЬ."); break;
                    case "ExitNotice": MakeCard(name, "ИГРА РАБОТАЕТ В БРАУЗЕРЕ", "ЗАКРОЙТЕ ВКЛАДКУ, ЧТОБЫ ВЫЙТИ."); break;
                    default: MakeButton(name, name, true); break;
                }
            }
            var texture = Images[name];
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
            Sprites[name] = sprite;
            return sprite;
        }

        private static void MakeButton(string name, string label, bool secondary)
        {
            
            var p = new PixelCanvas(480, 90, new Color32(0, 0, 0, 0));
            Color32 border = secondary ? C(96, 173, 190) : C(230, 159, 114);
            p.Rect(5, 6, 470, 78, C(8, 14, 21));
            p.Rect(9, 10, 462, 70, C(28, 43, 55));
            p.Rect(9, 76, 462, 4, C(8, 12, 20));
            p.Rect(5, 6, 470, 4, border);
            p.Rect(5, 80, 470, 4, border);
            p.Rect(5, 6, 4, 78, border);
            p.Rect(471, 6, 4, 78, border);
            p.Rect(22, 24, 3, 42, secondary ? C(81, 135, 143) : C(136, 76, 65));
            p.TextCentered(label, 5, C(244, 225, 199), 45);
            p.Save(name);
        }

        private static void MakeCard(string name, string a, string b)
        {
            
            var p = new PixelCanvas(1150, 400, C(8, 16, 25));
            p.Rect(8, 8, 1134, 384, C(29, 38, 49));
            p.Rect(15, 15, 1120, 5, C(208, 143, 97));
            p.Rect(15, 381, 1120, 4, C(76, 119, 136));
            for (int i = 0; i < 100; i++)
                p.Rect(22 + (i * 73 % 1090), 35 + (i * 59 % 325), 2, 2, C(51, 63, 70));
            p.TextCentered(a, 8, C(233, 163, 111), 124);
            p.TextCentered(b, 5, C(232, 226, 214), 260);
            p.Save(name);
        }

        private static void MakeBanner(string a, string b, string name, Color32 accent)
        {
            int width = name == "StudioLogo" ? 870 : 1200;
            var p = new PixelCanvas(width, 360, new Color32(0, 0, 0, 0));
            if (name == "StudioLogo")
            {
                p.Rect(178, 110, width - 356, 4, C(215, 165, 123));
                p.TextCentered("JAH NOMAD", 8, C(42, 34, 54), 186, 5, 6);
                p.TextCentered("JAH NOMAD", 8, accent, 180);
                p.TextCentered("НЕЗАВИСИМАЯ ИГРОВАЯ СТУДИЯ", 2, C(237, 218, 192), 277);
            }
            else
            {
                p.Rect(70, 33, 312, 3, C(218, 163, 105));
                p.Rect(width - 382, 33, 312, 3, C(218, 163, 105));
                p.TextCentered("JAH NOMAD", 3, C(242, 221, 186), 38);
                p.TextCentered(a, 12, C(85, 33, 57), 161, 8, 10);
                p.TextCentered(a, 12, accent, 148);
                p.Rect(60, 279, 140, 3, C(53, 187, 209));
                p.Rect(width - 200, 279, 140, 3, C(53, 187, 209));
                p.TextCentered(b, 10, C(25, 57, 83), 270, 7, 8);
                p.TextCentered(b, 10, C(245, 233, 204), 260);
            }
            p.Save(name);
        }

        private static PixelCanvas MakeBackground()
        {
            var p = new PixelCanvas(640, 360, C(8, 14, 30));
            var random = new System.Random(1996);
            for (int y = 0; y < 360; y++) {
                float f = y / 360f;
                var c = C((byte)(7 + f * 20), (byte)(13 + f * 11), (byte)(30 + f * 12));
                p.Rect(0, y, 640, 1, c);
            }
            for (int i = 0; i < 350; i++) {
                int sx = random.Next(640), sy = random.Next(210);
                p.Rect(sx, sy, random.Next(1, 3), 1, i % 6 == 0 ? C(188, 215, 229) : C(79, 135, 157));
            }
            p.Circle(531, 53, 21, C(117, 136, 147));
            p.Circle(523, 47, 16, C(167, 172, 168));
            p.Rect(0, 274, 640, 86, C(29, 37, 42));
            p.Rect(0, 301, 640, 2, C(46, 50, 48));
            // Distinctive Soviet apartment blocks. Static structures, separate from playable objects.
            Block(p, 18, 81, 146, 196, 6, 10, random);
            Block(p, 170, 112, 137, 165, 6, 8, random);
            Block(p, 314, 58, 153, 219, 6, 11, random);
            Block(p, 475, 130, 154, 147, 7, 7, random);
            p.Rect(304, 275, 173, 7, C(11, 21, 26));
            p.Rect(0, 302, 640, 2, C(77, 72, 66));
            // Wet puddles and neon reflections.
            for (int i = 0; i < 46; i++) {
                int px = random.Next(10, 620), py = random.Next(288, 350);
                p.Rect(px, py, random.Next(4, 29), random.Next(1, 3),
                    i % 3 == 0 ? C(92, 66, 66) : C(40, 66, 78));
            }
            // Lonely streetlight and warm pool of light.
            p.Rect(95, 197, 4, 96, C(60, 64, 61));
            p.Rect(86, 192, 23, 6, C(53, 60, 64));
            p.Rect(91, 194, 12, 5, C(248, 183, 94));
            for (int i = 0; i < 7; i++)
                p.Rect(92 - i * 5, 286 + i * 2, 15 + i * 10, 2, C(70 + i * 4, 52 + i * 2, 45));
            // Late-90s kiosk, glowing sign, panel joints and small bench.
            p.Rect(482, 232, 91, 47, C(46, 53, 57));
            p.Rect(488, 237, 79, 27, C(106, 75, 69));
            p.Rect(489, 239, 77, 5, C(184, 85, 74));
            p.Rect(488, 235, 79, 19, C(75, 34, 44));
            p.TextAt("ЛАРЁК", 2, C(255, 206, 143), 498, 237);
            p.Rect(489, 256, 77, 20, C(242, 179, 100));
            p.Rect(493, 259, 69, 15, C(54, 40, 47));
            for (int i = 0; i < 18; i++)
                p.Rect(495 + (i % 9) * 7, 262 + (i / 9) * 7, 3 + i % 2, 5,
                    i % 3 == 0 ? C(214, 102, 66) : i % 3 == 1 ? C(224, 190, 111) : C(89, 166, 177));
            p.Rect(563, 278, 4, 22, C(26, 29, 30));
            p.Rect(391, 296, 63, 5, C(53, 44, 44));
            p.Rect(398, 302, 4, 9, C(53, 44, 44)); p.Rect(444, 302, 4, 9, C(53, 44, 44));
            for (int i = 0; i < 26; i++) {
                int px = random.Next(0, 640), py = random.Next(278, 360);
                p.Rect(px, py, 2, 1, C(62, 73, 69));
            }
            return p;
        }

        private static void Block(PixelCanvas p, int x, int y, int w, int h, int cols, int rows, System.Random r)
        {
            p.Rect(x + 5, y + 5, w, h, C(12, 17, 29));
            p.Rect(x, y, w, h, C(43, 49, 58));
            p.Rect(x, y, w, 5, C(63, 64, 65));
            p.Rect(x + 1, y + 7, 4, h - 9, C(67, 67, 67));
            for (int i = 1; i < 5; i++)
                p.Rect(x + 4 + i * (w / 5), y + 5, 1, h - 7, C(49, 53, 58));
            for (int row = 0; row < rows; row++)
                for (int col = 0; col < cols; col++) {
                    int px = x + 11 + col * ((w - 20) / cols), py = y + 14 + row * ((h - 21) / rows);
                    Color32 c = r.Next(5) == 0 ? C(221, 142, 82) :
                                  r.Next(5) == 0 ? C(116, 145, 158) : C(18, 28, 39);
                    p.Rect(px, py, 11, 10, C(17, 26, 34));
                    p.Rect(px + 2, py + 2, 7, 6, c);
                }
        }

        private static Color32 C(byte r, byte g, byte b, byte a = 255) => new Color32(r, g, b, a);
        private static Color32 C(int r, int g, int b, int a = 255) =>
            new Color32((byte)r, (byte)g, (byte)b, (byte)a);

        private sealed partial class PixelCanvas
        {
            private readonly int w, h;
            private readonly Color32[] pixels;
            private static readonly Dictionary<char, string[]> Glyph = BuildGlyphs();
            public PixelCanvas(int width, int height, Color32 bg) {
                w = width; h = height; pixels = new Color32[w * h];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = bg;
            }
            public void Rect(int x, int y, int width, int height, Color32 c) {
                int x1 = Mathf.Clamp(x, 0, w), x2 = Mathf.Clamp(x + width, 0, w);
                int y1 = Mathf.Clamp(y, 0, h), y2 = Mathf.Clamp(y + height, 0, h);
                for (int iy = y1; iy < y2; iy++)
                    for (int ix = x1; ix < x2; ix++) pixels[(h - 1 - iy) * w + ix] = c;
            }
            public void Circle(int x, int y, int radius, Color32 c) {
                for (int yy = -radius; yy <= radius; yy++)
                    for (int xx = -radius; xx <= radius; xx++)
                        if (xx * xx + yy * yy <= radius * radius) Rect(x + xx, y + yy, 1, 1, c);
            }
            public void TextCentered(string text, int scale, Color32 color, int centerY,
                                      int offsetX = 0, int offsetY = 0) {
                int total = text.Length * 6 * scale;
                int sx = (w - total) / 2 + offsetX;
                int sy = centerY - (7 * scale) / 2 + offsetY;
                for (int i = 0; i < text.Length; i++) {
                    if (!Glyph.TryGetValue(char.ToUpperInvariant(text[i]), out var g)) continue;
                    for (int row = 0; row < 7; row++)
                        for (int col = 0; col < 5; col++)
                            if (g[row][col] == '1')
                                Rect(sx + i * 6 * scale + col * scale, sy + row * scale, scale, scale, color);
                }
            }
            public void Save(string name) {
                var t = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
                t.SetPixels32(pixels);
                t.Apply(false);
                t.filterMode = FilterMode.Point;
                t.wrapMode = TextureWrapMode.Clamp;
                Images[name] = t;
            }
            private static Dictionary<char, string[]> BuildGlyphs() {
                var d = new Dictionary<char, string[]>();
                void Add(char c, string s) { d[c] = s.Split('/'); }
                Add('А',"01110/10001/10001/11111/10001/10001/10001");
                Add('Б',"11111/10000/10000/11110/10001/10001/11110");
                Add('В',"11110/10001/10001/11110/10001/10001/11110");
                Add('Г',"11111/10000/10000/10000/10000/10000/10000");
                Add('Д',"00111/01001/01001/01001/01001/11111/10001");
                Add('Е',"11111/10000/10000/11110/10000/10000/11111");
                Add('Ё',"01010/00000/11111/10000/11110/10000/11111");
                Add('Ж',"10101/10101/10101/01110/10101/10101/10101");
                Add('З',"11110/00001/00001/00110/00001/00001/11110");
                Add('И',"10001/10011/10101/10101/11001/10001/10001");
                Add('Й',"10101/10001/10011/10101/10101/11001/10001");
                Add('К',"10001/10010/10100/11000/10100/10010/10001");
                Add('Л',"00111/01001/01001/01001/01001/10001/10001");
                Add('М',"10001/11011/10101/10101/10001/10001/10001");
                Add('Н',"10001/10001/10001/11111/10001/10001/10001");
                Add('О',"01110/10001/10001/10001/10001/10001/01110");
                Add('П',"11111/10001/10001/10001/10001/10001/10001");
                Add('Р',"11110/10001/10001/11110/10000/10000/10000");
                Add('С',"01111/10000/10000/10000/10000/10000/01111");
                Add('Т',"11111/00100/00100/00100/00100/00100/00100");
                Add('У',"10001/10001/01010/00100/00100/01000/10000");
                Add('Ф',"00100/01110/10101/10101/01110/00100/00100");
                Add('Х',"10001/10001/01010/00100/01010/10001/10001");
                Add('Ц',"10010/10010/10010/10010/10010/11111/00001");
                Add('Ч',"10001/10001/10001/01111/00001/00001/00001");
                Add('Ш',"10101/10101/10101/10101/10101/10101/11111");
                Add('Щ',"10101/10101/10101/10101/10101/11111/00001");
                Add('Ъ',"11000/01000/01110/01001/01001/01001/01110");
                Add('Ы',"10001/10001/10001/11101/10101/10101/11101");
                Add('Ь',"10000/10000/10000/11110/10001/10001/11110");
                Add('Э',"11110/00001/00001/00111/00001/00001/11110");
                Add('Ю',"10010/10101/10101/11101/10101/10101/10010");
                Add('Я',"01111/10001/10001/01111/00101/01001/10001");
                Add('0',"01110/10011/10101/10101/10101/11001/01110");
                Add('1',"00100/01100/00100/00100/00100/00100/01110");
                Add('2',"01110/10001/00001/00010/00100/01000/11111");
                Add('3',"11110/00001/00001/00110/00001/00001/11110");
                Add('4',"00010/00110/01010/10010/11111/00010/00010");
                Add('5',"11111/10000/10000/11110/00001/00001/11110");
                Add('6',"01110/10000/10000/11110/10001/10001/01110");
                Add('7',"11111/00001/00010/00100/01000/01000/01000");
                Add('8',"01110/10001/10001/01110/10001/10001/01110");
                Add('9',"01110/10001/10001/01111/00001/00001/01110");
                Add('J',"00111/00010/00010/00010/10010/10010/01100");
                Add('A',"01110/10001/10001/11111/10001/10001/10001");
                Add('H',"10001/10001/10001/11111/10001/10001/10001");
                Add('N',"10001/11001/10101/10101/10011/10001/10001");
                Add('O',"01110/10001/10001/10001/10001/10001/01110");
                Add('M',"10001/11011/10101/10101/10001/10001/10001");
                Add('D',"11110/10001/10001/10001/10001/10001/11110");
                Add(' ',"00000/00000/00000/00000/00000/00000/00000");
                Add('.',"00000/00000/00000/00000/00000/00110/00110");
                Add(':',"00000/00110/00110/00000/00110/00110/00000");
                Add('?',"01110/10001/00001/00010/00100/00000/00100");
                Add(',',"00000/00000/00000/00000/00110/00110/00100");
                Add('-',"00000/00000/00000/11111/00000/00000/00000");
                Add('/',"00001/00001/00010/00100/01000/10000/10000");
                return d;
            }
        }
    }
}
