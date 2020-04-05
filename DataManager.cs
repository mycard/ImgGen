namespace ImgGen
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.IO;
    using System.Text.RegularExpressions;

    public class DataManager
    {
        private static Bitmap[] bAttributes = new Bitmap[8];
        private static Bitmap[] bStar = new Bitmap[2];
        private static Bitmap[] bTemplates = new Bitmap[15];
        private static Bitmap[] bType = new Bitmap[9];
        private static Bitmap[] bLinkNums = new Bitmap[8];
        private static Bitmap[] bLinkMarkers = new Bitmap[9];

        private static Dictionary<int, Data> cardDatas = new Dictionary<int, Data>();
        private static Dictionary<int, Bitmap> cardImages = new Dictionary<int, Bitmap>();

        private static SQLiteConnection conn;
        private static object locker = new object();

        private static Font nameFont;
        private static Font numFont;
        private static Font numUnknownFont;
        private static Font txtFont;
        private static Font typeFont;
        private static Font scaleFontNormal;
        private static Font scaleFontSmall;
        private static SolidBrush nameShadowBrush;
        private static SolidBrush pendBgBrush;
        private static SolidBrush textBrush;

        private static StringFormat justifyFormat;
        private static StringFormat rightAlignFormat;

        private static string regex_monster = @"[果|介|述|報]】\n([\S\s]*)";
        private static string regex_pendulum = @"】[\s\S]*?\n([\S\s]*?)\n【";
        private static string non_start_chars = @"。；：，、”」）·× ";
        private static string non_end_chars = @"“「（●";

        private static string xyzString = "超量";
        private static string fontName = "文泉驿微米黑";
        private static string numfontName = "MatrixBoldSmallCaps";
        private static string spfontName = "黑体";

        private static List<int> zeroStarCards = new List<int>();

        public static void InitialDatas(string dbPath)
        {
            string _xyzString = System.Configuration.ConfigurationManager.AppSettings["XyzString"];
            if (_xyzString != null)
                xyzString = _xyzString;

            string _fontName = System.Configuration.ConfigurationManager.AppSettings["FontName"];
            if (_fontName != null)
                fontName = _fontName;

            string _zeroStarCards = System.Configuration.ConfigurationManager.AppSettings["ZeroStarCards"];
            if (_zeroStarCards != null)
            {
                foreach (string i in _zeroStarCards.Split(','))
                {
                    zeroStarCards.Add(int.Parse(i));
                }
            }

            conn = new SQLiteConnection("Data Source=" + dbPath);
            numFont = new Font(numfontName, 17, FontStyle.Bold, GraphicsUnit.Pixel);
            numUnknownFont = new Font(numfontName, 22, FontStyle.Bold, GraphicsUnit.Pixel);
            nameFont = new Font(fontName, 28, GraphicsUnit.Pixel);
            typeFont = new Font(fontName, 12, FontStyle.Bold, GraphicsUnit.Pixel);
            txtFont = new Font(fontName, 10, FontStyle.Bold, GraphicsUnit.Pixel);
            scaleFontNormal = new Font(numfontName, 30, GraphicsUnit.Pixel);
            scaleFontSmall = new Font(numfontName, 27, GraphicsUnit.Pixel);

            nameShadowBrush = new SolidBrush(Color.FromArgb(64, 64, 0));
            pendBgBrush = new SolidBrush(Color.FromArgb(0, 125, 105));
            textBrush = new SolidBrush(Color.FromArgb(0, 0, 0));

            justifyFormat = new StringFormat(StringFormat.GenericTypographic);
            justifyFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            justifyFormat.FormatFlags |= StringFormatFlags.FitBlackBox;
            rightAlignFormat = new StringFormat();
            rightAlignFormat.Alignment = StringAlignment.Far;

            bTemplates[0] = new Bitmap("./textures/card_spell.png");
            bTemplates[1] = new Bitmap("./textures/card_trap.png");
            bTemplates[2] = new Bitmap("./textures/card_synchro.png");
            bTemplates[3] = new Bitmap("./textures/card_xyz.png");
            bTemplates[4] = new Bitmap("./textures/card_fusion.png");
            bTemplates[5] = new Bitmap("./textures/card_ritual.png");
            bTemplates[6] = new Bitmap("./textures/card_token.png");
            bTemplates[7] = new Bitmap("./textures/card_effect.png");
            bTemplates[8] = new Bitmap("./textures/card_normal.png");
            bTemplates[9] = new Bitmap("./textures/card_pxyz.png");
            bTemplates[10] = new Bitmap("./textures/card_peffect.png");
            bTemplates[11] = new Bitmap("./textures/card_pnormal.png");
            bTemplates[12] = new Bitmap("./textures/card_psynchro.png");
            bTemplates[13] = new Bitmap("./textures/card_pfusion.png");
            bTemplates[14] = new Bitmap("./textures/card_link.png");
            bAttributes[0] = new Bitmap("./textures/att_earth.png");
            bAttributes[1] = new Bitmap("./textures/att_water.png");
            bAttributes[2] = new Bitmap("./textures/att_fire.png");
            bAttributes[3] = new Bitmap("./textures/att_wind.png");
            bAttributes[4] = new Bitmap("./textures/att_light.png");
            bAttributes[5] = new Bitmap("./textures/att_dark.png");
            bAttributes[6] = new Bitmap("./textures/att_devine.png");
            bStar[0] = new Bitmap("./textures/star.png");
            bStar[1] = new Bitmap("./textures/starb.png");
            bType[0] = new Bitmap("./textures/spell_normal.png");
            bType[1] = new Bitmap("./textures/spell_quickplay.png");
            bType[2] = new Bitmap("./textures/spell_continuous.png");
            bType[3] = new Bitmap("./textures/spell_equip.png");
            bType[4] = new Bitmap("./textures/spell_field.png");
            bType[5] = new Bitmap("./textures/spell_ritual.png");
            bType[6] = new Bitmap("./textures/trap_normal.png");
            bType[7] = new Bitmap("./textures/trap_continuous.png");
            bType[8] = new Bitmap("./textures/trap_counter.png");
            for (int i = 1; i <= 9; i++)
            {
                if (i < 9)
                    bLinkNums[i - 1] = new Bitmap("./textures/link_" + i + ".png");
                if (i == 5) continue;
                bLinkMarkers[i - 1] = new Bitmap("./textures/link_marker_on_" + i + ".png");
            }
        }

        public static Bitmap GetImage(int code)
        {
            if (!cardImages.ContainsKey(code))
            {
                LoadCard(code);
            }
            return cardImages[code];
        }

        private static int LoadCard(int code)
        {
            lock (locker)
            {
                if (cardDatas.ContainsKey(code))
                {
                    return 0;
                }
                Data data = new Data();
                Text text = new Text();
                data.code = code;
                text.name = "???";
                text.text = "???";
                try
                {
                    conn.Open();
                    SQLiteCommand command = new SQLiteCommand(conn);
                    SQLiteDataReader reader;

                    command.CommandText = string.Format("select * from datas where id={0}", code);
                    reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        data.code = reader.GetInt32(0);
                        data.alias = reader.GetInt32(2);
                        data.setcode = reader.GetInt32(3);
                        data.type = (Type)reader.GetInt32(4);
                        data.attack = reader.GetInt32(5);
                        data.defence = reader.GetInt32(6);
                        data.level = reader.GetInt32(7);
                        data.race = (Race)reader.GetInt32(8);
                        data.attribute = (Attribute)reader.GetInt32(9);
                    }
                    reader.Close();

                    command.CommandText = string.Format("select * from texts where id={0}", code);
                    reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        text.name = reader.GetString(1);
                        text.text = reader.GetString(2);
                    }
                    reader.Close();
                }
                catch
                {
                }
                finally
                {
                    conn.Close();
                }
                cardDatas.Add(code, data);
                if (!cardImages.ContainsKey(code))
                {
                    Bitmap bitmap;
                    bitmap = DrawCard(data, text);
                    cardImages.Add(code, bitmap);
                }
                return 0;
            }
        }

        private static string FormatCardDesc(string r)
        {
            char[] chArray = r.ToCharArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                if ((chArray[i] > ' ') && (chArray[i] < '\x007f')) // 半角转全角
                {
                    chArray[i] = (char)(chArray[i] + 0xfee0);
                }
                if (chArray[i] == '\x00b7') // another middle dot
                {
                    chArray[i] = '・';
                }
            }
            string desc = new string(chArray);
            desc = desc.Replace(Environment.NewLine, "\n");
            desc = Regex.Replace(desc, @"(?<=。)([\n\s]+)(?=[①②③④⑤⑥⑦⑧⑨⑩])", ""); // 去掉效果编号前的换行
            desc = Regex.Replace(desc, @"([\n\s]+)(?=●)", ""); // 去掉●号前的换行
            return desc;
        }

        private static string FormatCardName(string r)
        {
            return r.Replace('\x00b7', '・'); // another middle dot
        }

        private static string GetMonsterDesc(string desc, string regex)
        {
            // 拆分灵摆卡的灵摆效果和怪兽效果
            Regex r = new Regex(regex, RegexOptions.Multiline);
            Match match = r.Match(desc);
            if (match.Success)
                return (match.Groups.Count > 1) ? match.Groups[1].Value : match.Groups[0].Value;
            return "";
        }

        private static void DrawPicture(Graphics graphics, Data data)
        {
            Bitmap image;
            string filename = "./pico/" + data.code.ToString() + ".png";
            if (!File.Exists(filename))
                filename = "./pico/" + data.code.ToString() + ".jpg";
            try
            {
                image = new Bitmap(filename);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error prasing {0} {1}", data.code, e);
                return;
            }
            if (data.isType(Type.TYPE_PENDULUM))
            {
                if (image.Width == 347 && image.Height == 444)
                    graphics.DrawImage(image, 27, 103, 347, 444);
                else
                {
                    graphics.FillRectangle(pendBgBrush, new Rectangle(23, 362, 354, 189));
                    float ar = image.Width / image.Height;
                    if ((ar >= 1.3) && (ar <= 1.4))
                        graphics.DrawImage(image, 27, 103, 347, 259);
                    else
                        graphics.DrawImage(image, new Rectangle(27, 103, 347, 259), new Rectangle(0, 0, image.Width, image.Width * 259 / 347), GraphicsUnit.Pixel);
                }
            }
            else
            {
                graphics.DrawImage(image, 48, 106, 304, 304);
            }
        }

        private static void DrawTemplate(Graphics graphics, Data data)
        {
            Bitmap template;
            if (data.isType(Type.TYPE_SPELL))
            {
                template = new Bitmap(bTemplates[0]);
            }
            else if (data.isType(Type.TYPE_TRAP))
            {
                template = new Bitmap(bTemplates[1]);
            }
            else if (data.isType(Type.TYPE_PENDULUM))
            {
                if (data.isType(Type.TYPE_SYNCHRO))
                {
                    template = new Bitmap(bTemplates[12]);
                }
                else if (data.isType(Type.TYPE_XYZ))
                {
                    template = new Bitmap(bTemplates[9]);
                }
                else if (data.isType(Type.TYPE_FUSION))
                {
                    template = new Bitmap(bTemplates[13]);
                }
                else if (data.isType(Type.TYPE_EFFECT))
                {
                    template = new Bitmap(bTemplates[10]);
                }
                else //pnormal
                {
                    template = new Bitmap(bTemplates[11]);
                }
            }
            else if (data.isType(Type.TYPE_LINK))
            {
                template = new Bitmap(bTemplates[14]);
            }
            else if (data.isType(Type.TYPE_SYNCHRO))
            {
                template = new Bitmap(bTemplates[2]);
            }
            else if (data.isType(Type.TYPE_XYZ))
            {
                template = new Bitmap(bTemplates[3]);
            }
            else if (data.isType(Type.TYPE_FUSION))
            {
                template = new Bitmap(bTemplates[4]);
            }
            else if (data.isType(Type.TYPE_RITUAL))
            {
                template = new Bitmap(bTemplates[5]);
            }
            else if (data.isType(Type.TYPE_TOKEN))
            {
                template = new Bitmap(bTemplates[6]);
            }
            else if (data.isType(Type.TYPE_EFFECT))
            {
                template = new Bitmap(bTemplates[7]);
            }
            else //normal
            {
                template = new Bitmap(bTemplates[8]);
            }
            graphics.DrawImage(template, 0, 0, 400, 580);
        }

        private static void DrawStars(Graphics graphics, Data data)
        {
            if (!zeroStarCards.Contains(data.code))
            {
                int nStar;
                int level = data.level & 0xff;
                if (data.isType(Type.TYPE_XYZ))
                {
                    for (nStar = 0; nStar < level; nStar++)
                    {
                        graphics.DrawImage(bStar[1], 41f + (26.5f * nStar), 69, 28, 28);
                    }
                }
                else if (!data.isType(Type.TYPE_LINK))
                {
                    for (nStar = 0; nStar < level; nStar++)
                    {
                        graphics.DrawImage(bStar[0], 332f - (26.5f * nStar), 69, 28, 28);
                    }
                }
            }
        }

        private static void DrawAttributes(Graphics graphics, Data data)
        {
            int nAttr = -1;
            if (data.attribute == Attribute.ATTRIBUTE_EARTH) nAttr = 0;
            else if (data.attribute == Attribute.ATTRIBUTE_WATER) nAttr = 1;
            else if (data.attribute == Attribute.ATTRIBUTE_FIRE) nAttr = 2;
            else if (data.attribute == Attribute.ATTRIBUTE_WIND) nAttr = 3;
            else if (data.attribute == Attribute.ATTRIBUTE_LIGHT) nAttr = 4;
            else if (data.attribute == Attribute.ATTRIBUTE_DARK) nAttr = 5;
            else if (data.attribute == Attribute.ATTRIBUTE_DEVINE) nAttr = 6;
            if (nAttr >= 0)
                graphics.DrawImage(bAttributes[nAttr], 334, 28, 36, 36);
        }

        private static void DrawAtkDef(Graphics graphics, Data data)
        {
            if (data.attack >= 0)
            {
                graphics.DrawString(data.attack.ToString(), numFont, textBrush, new Rectangle(248, 530, 42, 17), rightAlignFormat);
            }
            else
            {
                graphics.DrawString("?", numUnknownFont, textBrush, 274, 527);
            }

            if (data.isType(Type.TYPE_LINK))
            {
                graphics.DrawImage(bLinkNums[data.level - 1], 353, 530, 13, 13);
            }
            else
            {
                if (data.defence >= 0)
                {
                    graphics.DrawString(data.defence.ToString(), numFont, textBrush, new Rectangle(329, 530, 42, 17), rightAlignFormat);
                }
                else
                {
                    graphics.DrawString("?", numUnknownFont, textBrush, 355, 527);
                }
            }
        }

        private static void DrawMonsterType(Graphics graphics, Data data)
        {
            string str = "【";
            switch (data.race)
            {
                case Race.RACE_WARRIOR: str += "战士族"; break;
                case Race.RACE_SPELLCASTER: str += "魔法师族"; break;
                case Race.RACE_FAIRY: str += "天使族"; break;
                case Race.RACE_FIEND: str += "恶魔族"; break;
                case Race.RACE_ZOMBIE: str += "不死族"; break;
                case Race.RACE_MACHINE: str += "机械族"; break;
                case Race.RACE_AQUA: str += "水族"; break;
                case Race.RACE_PYRO: str += "炎族"; break;
                case Race.RACE_ROCK: str += "岩石族"; break;
                case Race.RACE_WINDBEAST: str += "鸟兽族"; break;
                case Race.RACE_PLANT: str += "植物族"; break;
                case Race.RACE_INSECT: str += "昆虫族"; break;
                case Race.RACE_THUNDER: str += "雷族"; break;
                case Race.RACE_DRAGON: str += "龙族"; break;
                case Race.RACE_BEAST: str += "兽族"; break;
                case Race.RACE_BEASTWARRIOR: str += "兽战士族"; break;
                case Race.RACE_DINOSAUR: str += "恐龙族"; break;
                case Race.RACE_FISH: str += "鱼族"; break;
                case Race.RACE_SEASERPENT: str += "海龙族"; break;
                case Race.RACE_REPTILE: str += "爬虫族"; break;
                case Race.RACE_PSYCHO: str += "念动力族"; break;
                case Race.RACE_DEVINE: str += "幻神兽族"; break;
                case Race.RACE_CREATORGOD: str += "创造神族"; break;
                case Race.RACE_WYRM: str += "幻龙族"; break;
                case Race.RACE_CYBERS: str += "电子界族"; break;
                default: str += "???"; break;
            }
            if (data.isType(Type.TYPE_FUSION)) str += "／融合";
            if (data.isType(Type.TYPE_SYNCHRO)) str += "／同调";
            if (data.isType(Type.TYPE_LINK)) str += "／连接";
            if (data.isType(Type.TYPE_XYZ)) str = str + "／" + xyzString;
            if (data.isType(Type.TYPE_RITUAL)) str += "／仪式";
            if (data.isType(Type.TYPE_SPSUMMON)) str += "／特殊召唤";
            if (data.isType(Type.TYPE_PENDULUM)) str += "／灵摆";
            if (data.isType(Type.TYPE_SPIRIT)) str += "／灵魂";
            if (data.isType(Type.TYPE_DUAL)) str += "／二重";
            if (data.isType(Type.TYPE_UNION)) str += "／同盟";
            if (data.isType(Type.TYPE_FLIP)) str += "／反转";
            if (data.isType(Type.TYPE_TOON)) str += "／卡通";
            if (data.isType(Type.TYPE_TUNER)) str += "／调整";
            if (data.isType(Type.TYPE_EFFECT)) str += "／效果";
            if (data.isType(Type.TYPE_NORMAL)) str += "／通常";
            str += "】";

            float tWidth = graphics.MeasureString(str, typeFont).Width;
            float sx1 = 1f;
            if (tWidth > 330f)
            {
                sx1 *= 330f / tWidth;
            }
            graphics.ScaleTransform(sx1, 1f);
            graphics.DrawString(str, typeFont, textBrush, 26, 438);
            graphics.ResetTransform();
        }

        private static void DrawScales(Graphics graphics, Data data)
        {
            int lscale = (data.level >> 0x18) & 0xff;
            int rscale = (data.level >> 0x10) & 0xff;
            if (lscale > 9)
            {
                graphics.DrawString("1", scaleFontSmall, textBrush, 26, 397);
                graphics.DrawString((lscale - 10).ToString(), scaleFontSmall, textBrush, 37, 397);
            }
            else
            {
                graphics.DrawString(lscale.ToString(), scaleFontNormal, textBrush, 31, 396);
            }
            if (rscale > 9)
            {
                graphics.DrawString("1", scaleFontSmall, textBrush, 341, 397);
                graphics.DrawString((rscale - 10).ToString(), scaleFontSmall, textBrush, 352, 397);
            }
            else
            {
                graphics.DrawString(rscale.ToString(), scaleFontNormal, textBrush, 346, 396);
            }
        }

        private static void DrawMonsterEffect(Graphics graphics, string text)
        {
            DrawJustifiedText(graphics, text, 33, 453, 335, 75);
        }

        private static void DrawPendulumEffect(Graphics graphics, string text)
        {
            DrawJustifiedText(graphics, text, 65, 369, 272, 58);
        }

        private static void DrawSpellTrapEffect(Graphics graphics, string text)
        {
            DrawJustifiedText(graphics, text, 33, 439, 335, 108);
        }

        private static void DrawSpellTrapType(Graphics graphics, Data data)
        {
            if (data.isType(Type.TYPE_SPELL))
            {
                int nType = 0;
                if (data.isType(Type.TYPE_QUICKPLAY)) nType = 1;
                if (data.isType(Type.TYPE_CONTINUOUS)) nType = 2;
                if (data.isType(Type.TYPE_EQUIP)) nType = 3;
                if (data.isType(Type.TYPE_FIELD)) nType = 4;
                if (data.isType(Type.TYPE_RITUAL)) nType = 5;
                graphics.DrawImage(bType[nType], 221, 69, 137, 26);
            }
            else if (data.isType(Type.TYPE_TRAP))
            {
                int nType = 6;
                if (data.isType(Type.TYPE_CONTINUOUS)) nType = 7;
                if (data.isType(Type.TYPE_COUNTER)) nType = 8;
                graphics.DrawImage(bType[nType], 243, 68, 115, 27);
            }
        }

        private static void DrawLinkMarkers(Graphics graphics, Data data)
        {
            LinkMarker lm = (LinkMarker)data.defence;
            if ((lm & LinkMarker.LINK_MARKER_BOTTOM_LEFT) > 0)
                graphics.DrawImage(bLinkMarkers[0], 34, 387, 38, 37);
            if ((lm & LinkMarker.LINK_MARKER_BOTTOM) > 0)
                graphics.DrawImage(bLinkMarkers[1], 163, 406, 73, 25);
            if ((lm & LinkMarker.LINK_MARKER_BOTTOM_RIGHT) > 0)
                graphics.DrawImage(bLinkMarkers[2], 329, 387, 37, 37);
            if ((lm & LinkMarker.LINK_MARKER_LEFT) > 0)
                graphics.DrawImage(bLinkMarkers[3], 27, 222, 24, 72);
            if ((lm & LinkMarker.LINK_MARKER_RIGHT) > 0)
                graphics.DrawImage(bLinkMarkers[5], 349, 221, 24, 72);
            if ((lm & LinkMarker.LINK_MARKER_TOP_LEFT) > 0)
                graphics.DrawImage(bLinkMarkers[6], 34, 91, 37, 37);
            if ((lm & LinkMarker.LINK_MARKER_TOP) > 0)
                graphics.DrawImage(bLinkMarkers[7], 163, 85, 74, 23);
            if ((lm & LinkMarker.LINK_MARKER_TOP_RIGHT) > 0)
                graphics.DrawImage(bLinkMarkers[8], 329, 91, 37, 37);
        }

        private static void DrawName(Graphics graphics, string name)
        {
            float width = graphics.MeasureString(name, nameFont).Width;
            float sx = 1f;
            if (width > 310f)
            {
                sx *= 310f / width;
            }
            graphics.TranslateTransform(26, 28);
            graphics.ScaleTransform(sx, 1f);
            graphics.DrawString(name, nameFont, nameShadowBrush, 0f, 0f);
            graphics.DrawString(name, nameFont, Brushes.Gold, 1f, 1f);
            graphics.ResetTransform();
        }

        private static Bitmap DrawCard(Data data, Text text)
        {
            Bitmap bitmap = new Bitmap(400, 580);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            string name = FormatCardName(text.name);
            string desc = FormatCardDesc(text.text);

            DrawPicture(graphics, data);
            DrawTemplate(graphics, data);

            DrawName(graphics, name);

            if (data.isType(Type.TYPE_MONSTER))
            {
                DrawStars(graphics, data);
                DrawAttributes(graphics, data);
                DrawMonsterType(graphics, data);
                DrawAtkDef(graphics, data);

                if (data.isType(Type.TYPE_PENDULUM))
                {
                    DrawScales(graphics, data);
                    DrawPendulumEffect(graphics, GetMonsterDesc(desc, regex_pendulum));
                    DrawMonsterEffect(graphics, GetMonsterDesc(desc, regex_monster));
                }
                else
                {
                    DrawMonsterEffect(graphics, desc);
                }

                if (data.isType(Type.TYPE_LINK))
                {
                    DrawLinkMarkers(graphics, data);
                }
            }
            else
            {
                DrawSpellTrapType(graphics, data);
                DrawSpellTrapEffect(graphics, desc);
            }

            return bitmap;
        }

        private static string GetNextWord(string text, int pos)
        {
            return pos < text.Length - 1 ? text.Substring(pos + 1, 1) : "咕";
        }

        private static float GetLineSpacing(float size)
        {
            return size / 5f;
        }

        private static void DrawJustifiedText(Graphics graphics, string text, float areaX, float areaY, float areaWidth, float areaHeight)
        {
            float size = txtFont.Size;
            var font = new Font(txtFont.Name, size, txtFont.Style, txtFont.Unit);
            List<string> lines = new List<string> { };
            List<float> paddings = new List<float> { }; // 每行文字两端对齐须补充的像素
            while (true)
            {
                // 自动缩小字体直到寻找到行数不溢出区域的值
                int pos = 0;
                string line = "";
                float linewidth = 0;
                while (pos < text.Length)
                {
                    string word = text.Substring(pos, 1);
                    string nextword = GetNextWord(text, pos);
                    if (word == "\n")
                    {
                        lines.Add(line);
                        paddings.Add(0);
                        line = "";
                        linewidth = 0;
                        pos++;
                        continue;
                    }
                    SizeF doublesize = graphics.MeasureString(word + nextword, font, 99, justifyFormat);
                    SizeF singlesize = graphics.MeasureString(word, font, 99, justifyFormat);
                    float wordwidth = doublesize.Width - singlesize.Width;
                    if (linewidth + wordwidth > areaWidth
                        || (non_start_chars.Contains(nextword) && linewidth + doublesize.Width > areaWidth)
                        || (non_end_chars.Contains(nextword) && linewidth + doublesize.Width < areaWidth && linewidth + doublesize.Width + size > areaWidth))
                    {
                        lines.Add(line);
                        paddings.Add(areaWidth - linewidth);
                        line = "";
                        linewidth = 0;
                    }
                    line += word;
                    linewidth += wordwidth;
                    pos++;
                }
                if (linewidth > 0)
                {
                    lines.Add(line);
                    paddings.Add(0);
                }
                if (lines.Count * (size + GetLineSpacing(size)) <= areaHeight - GetLineSpacing(size))
                    break;
                size -= 0.5f;
                font = new Font(txtFont.Name, size, txtFont.Style, txtFont.Unit);
                lines.Clear();
                paddings.Clear();
            }
            float dx = areaX;
            float dy = areaY;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                float exspace = paddings[i] / (line.Length - 1);
                for (int pos = 0; pos < line.Length; pos++)
                {
                    string word = line.Substring(pos, 1);
                    string nextword = GetNextWord(line, pos);

                    if (word == "●")
                    {
                        Font spFont = new Font(spfontName, size * 0.9f, txtFont.Style, txtFont.Unit);
                        graphics.DrawString(word, spFont, textBrush, dx + (size / 10f), dy + (size / 10f), justifyFormat);
                    }
                    else if (word == "×")
                    {
                        Font spFont = new Font(spfontName, size, txtFont.Style, txtFont.Unit);
                        graphics.DrawString(word, spFont, textBrush, dx + (size / 3f), dy + (size / 20f), justifyFormat);
                    }
                    else if (word == "量")
                    {
                        Font spFont = new Font(spfontName, size, txtFont.Style, txtFont.Unit);
                        graphics.DrawString(word, spFont, textBrush, dx, dy + (size / 20f), justifyFormat);
                    }
                    else
                        graphics.DrawString(word, font, textBrush, dx, dy, justifyFormat);

                    SizeF doublesize = graphics.MeasureString(word + nextword, font, 99, justifyFormat);
                    SizeF singlesize = graphics.MeasureString(word, font, 99, justifyFormat);
                    float dw = doublesize.Width - singlesize.Width;
                    dx += dw + exspace;
                }
                dx = areaX;
                dy += size + GetLineSpacing(size);
            }
        }

        public static Bitmap Zoom(Bitmap sourceBitmap, int newWidth, int newHeight)
        {
            if (sourceBitmap != null)
            {
                Bitmap b = new Bitmap(newWidth, newHeight);
                Graphics graphics = Graphics.FromImage(b);
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                Rectangle newRect = new Rectangle(0, 0, newWidth, newHeight);
                Rectangle srcRect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
                graphics.DrawImage(sourceBitmap, newRect, srcRect, GraphicsUnit.Pixel);
                graphics.Dispose();

                return b;
            }
            return null;
        }
    }
}

