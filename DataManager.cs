namespace ImgGen
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;

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
        private static SolidBrush nameShadowBrush = new SolidBrush(Color.FromArgb(64, 64, 0));
        private static SolidBrush pendBgBrush = new SolidBrush(Color.FromArgb(0, 125, 105));
        private static SolidBrush textBrush = new SolidBrush(Color.FromArgb(0, 0, 0));

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

        public static Bitmap GetImage(int code)
        {
            if (!cardImages.ContainsKey(code))
            {
                LoadCard(code);
            }
            return cardImages[code];
        }

        private static string GetTypeString(Data data)
        {
            string str = "【";
            switch (data.race)
            {
                case Race.RACE_WARRIOR: str = str + "战士族"; break;
                case Race.RACE_SPELLCASTER: str = str + "魔法师族"; break;
                case Race.RACE_FAIRY: str = str + "天使族"; break;
                case Race.RACE_FIEND: str = str + "恶魔族"; break;
                case Race.RACE_ZOMBIE: str = str + "不死族"; break;
                case Race.RACE_MACHINE: str = str + "机械族"; break;
                case Race.RACE_AQUA: str = str + "水族"; break;
                case Race.RACE_PYRO: str = str + "炎族"; break;
                case Race.RACE_ROCK: str = str + "岩石族"; break;
                case Race.RACE_WINDBEAST: str = str + "鸟兽族"; break;
                case Race.RACE_PLANT: str = str + "植物族"; break;
                case Race.RACE_INSECT: str = str + "昆虫族"; break;
                case Race.RACE_THUNDER: str = str + "雷族"; break;
                case Race.RACE_DRAGON: str = str + "龙族"; break;
                case Race.RACE_BEAST: str = str + "兽族"; break;
                case Race.RACE_BEASTWARRIOR: str = str + "兽战士族"; break;
                case Race.RACE_DINOSAUR: str = str + "恐龙族"; break;
                case Race.RACE_FISH: str = str + "鱼族"; break;
                case Race.RACE_SEASERPENT: str = str + "海龙族"; break;
                case Race.RACE_REPTILE: str = str + "爬虫族"; break;
                case Race.RACE_PSYCHO: str = str + "念动力族"; break;
                case Race.RACE_DEVINE: str = str + "幻神兽族"; break;
                case Race.RACE_CREATORGOD: str = str + "创造神族"; break;
                case Race.RACE_WYRM: str = str + "幻龙族"; break;
                case Race.RACE_CYBERS: str = str + "电子界族"; break;
                default: str = str + "???"; break;
            }
            if (data.isType(Type.TYPE_FUSION)) str = str + "／融合";
            if (data.isType(Type.TYPE_SYNCHRO)) str = str + "／同调";
            if (data.isType(Type.TYPE_LINK)) str = str + "／连接";
            if (data.isType(Type.TYPE_XYZ)) str = str + "／" + xyzString;
            if (data.isType(Type.TYPE_RITUAL)) str = str + "／仪式";
            if (data.isType(Type.TYPE_SPSUMMON)) str = str + "／特殊召唤";
            if (data.isType(Type.TYPE_PENDULUM)) str = str + "／灵摆";
            if (data.isType(Type.TYPE_SPIRIT)) str = str + "／灵魂";
            if (data.isType(Type.TYPE_DUAL)) str = str + "／二重";
            if (data.isType(Type.TYPE_UNION)) str = str + "／同盟";
            if (data.isType(Type.TYPE_FLIP)) str = str + "／反转";
            if (data.isType(Type.TYPE_TOON)) str = str + "／卡通";
            if (data.isType(Type.TYPE_TUNER)) str = str + "／调整";
            if (data.isType(Type.TYPE_EFFECT)) str = str + "／效果";
            if (data.isType(Type.TYPE_NORMAL)) str = str + "／通常";
            return (str + "】");
        }

        private static string GetStandardText(string r)
        {
            char[] chArray = r.ToCharArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                if ((chArray[i] > ' ') && (chArray[i] < '\x007f'))
                {
                    chArray[i] = (char)(chArray[i] + 0xfee0);
                }
                if (chArray[i] == '\x00b7')
                {
                    chArray[i] = '・';
                }
            }
            string desc = new string(chArray);
            desc = desc.Replace(Environment.NewLine, "\n");
            desc = Regex.Replace(desc, @"(?<=。)([\n\s]+)(?=[①②③④⑤⑥⑦⑧⑨⑩])", "");
            desc = Regex.Replace(desc, @"([\n\s]+)(?=●)", "");
            return desc;
        }

        private static string GetPendulumDesc(string cdesc, string regx)
        {
            string desc = cdesc;
            desc = desc.Replace(Environment.NewLine, "\n");
            Regex regex = new Regex(regx, RegexOptions.Multiline);
            Match mc = regex.Match(desc);
            if (mc.Success)
                return ((mc.Groups.Count > 1) ?
                        mc.Groups[1].Value : mc.Groups[0].Value);
            //.Trim('\n').Replace("\n", "\n\t\t");
            return "";
        }

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

            justifyFormat = new StringFormat(StringFormat.GenericTypographic);
            justifyFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            justifyFormat.FormatFlags |= StringFormatFlags.FitBlackBox;
            rightAlignFormat= new StringFormat();
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
                    SQLiteCommand command = new SQLiteCommand(string.Format("select * from datas where id={0}", code), conn);
                    SQLiteDataReader reader = command.ExecuteReader();
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
                    string str = string.Format("select * from texts where id={0}", code);
                    command.CommandText = str;
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
                    bitmap = DrawImage(code, data, text);
                    cardImages.Add(code, bitmap);
                }
                return 0;
            }
        }

        private static Bitmap DrawImage(int code, Data data, Text text)
        {
            Bitmap bitmap = new Bitmap(400, 580);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            try
            {
                Bitmap image = new Bitmap("./pico/" + code.ToString() + ".jpg");
                if (data.isType(Type.TYPE_PENDULUM))
                {
                    if (image.Width == 347 && image.Height == 444)
                        graphics.DrawImage(image, 26, 103, 347, 444);
                    else
                    {
                        graphics.FillRectangle(pendBgBrush, new Rectangle(23, 362, 354, 189));
                        float ar = image.Width / image.Height;
                        if ((ar >= 1.3) && (ar <= 1.4))
                            graphics.DrawImage(image, 26, 103, 347, 260);
                        else
                            graphics.DrawImage(image, new Rectangle(26, 103, 347, 260), new Rectangle(0, 0, image.Width, image.Width * 260 / 347), GraphicsUnit.Pixel);
                    }
                }
                else
                {
                    graphics.DrawImage(image, 48, 106, 304, 304);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error prasing {0} {1}", data.code, e);
            }


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


            text.text = GetStandardText(text.text);
            if (data.isType(Type.TYPE_MONSTER))
            {
                if (!zeroStarCards.Contains(data.code))
                {
                    int nStar;
                    int level = data.level & 0xff;
                    if (data.isType(Type.TYPE_XYZ))
                    {
                        for (nStar = 0; nStar < level; nStar++)
                        {
                            graphics.DrawImage(bStar[1], (int)41 + (26.5f * nStar), 69, 28, 28);
                        }
                    }
                    else if (!data.isType(Type.TYPE_LINK))
                    {
                        for (nStar = 0; nStar < level; nStar++)
                        {
                            graphics.DrawImage(bStar[0], (int)332 - (26.5f * nStar), 69, 28, 28);
                        }
                    }
                }

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

                if (data.attack >= 0)
                {
                    graphics.DrawString(data.attack.ToString(), numFont, Brushes.Black, new Rectangle(248, 530, 42, 17), rightAlignFormat);
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
                        graphics.DrawString(data.defence.ToString(), numFont, Brushes.Black, new Rectangle(329, 530, 42, 17), rightAlignFormat);
                    }
                    else
                    {
                        graphics.DrawString("?", numUnknownFont, textBrush, 355, 527);
                    }
                }

                string type_string = GetTypeString(data);
                float tWidth = graphics.MeasureString(type_string, typeFont).Width;
                float sx1 = 1f;
                if (tWidth > 330f)
                {
                    sx1 *= 330f / tWidth;
                }
                graphics.ScaleTransform(sx1, 1f);
                graphics.DrawString(type_string, typeFont, Brushes.Black, 26, 438);
                graphics.ResetTransform();

                string monster_effect = text.text;
                if (data.isType(Type.TYPE_PENDULUM))
                {
                    monster_effect = GetPendulumDesc(text.text, regex_monster);
                }
                DrawJustifiedText(graphics, monster_effect, 33, 453, 335, 75);

                if (data.isType(Type.TYPE_PENDULUM))
                {
                    int lscale = (data.level >> 0x18) & 0xff;
                    int rscale = (data.level >> 0x10) & 0xff;
                    if (lscale > 9)
                    {
                        graphics.DrawString("1", scaleFontSmall, Brushes.Black, 26, 397);
                        graphics.DrawString((lscale - 10).ToString(), scaleFontSmall, Brushes.Black, 37, 397);
                    }
                    else
                    {
                        graphics.DrawString(lscale.ToString(), scaleFontNormal, Brushes.Black, 31, 396);
                    }
                    if (rscale > 9)
                    {
                        graphics.DrawString("1", scaleFontSmall, Brushes.Black, 341, 397);
                        graphics.DrawString((rscale - 10).ToString(), scaleFontSmall, Brushes.Black, 352, 397);
                    }
                    else
                    {
                        graphics.DrawString(rscale.ToString(), scaleFontNormal, Brushes.Black, 346, 396);
                    }
                    string pendulum_effect = GetPendulumDesc(text.text, regex_pendulum);
                    DrawJustifiedText(graphics, pendulum_effect, 65, 369, 272, 58);
                }
            }
            else
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
                DrawJustifiedText(graphics, text.text, 33, 439, 335, 108);
            }
            if (data.isType(Type.TYPE_LINK))
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


            string nametext = text.name.Replace('\x00b7', '・');
            float width = graphics.MeasureString(nametext, nameFont).Width;
            float sx = 1f;
            if (width > 310f)
            {
                sx *= 310f / width;
            }
            graphics.TranslateTransform(26, 28);
            graphics.ScaleTransform(sx, 1f);
            graphics.DrawString(nametext, nameFont, nameShadowBrush, 0f, 0f);
            graphics.DrawString(nametext, nameFont, Brushes.Gold, 1f, 1f);
            graphics.ResetTransform();
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

        private static void DrawJustifiedText(Graphics graphics, string text, float x, float y, float w, float h)
        {
            float size = txtFont.Size;
            var font = new Font(txtFont.Name, size, txtFont.Style, txtFont.Unit);
            List<string> lines = new List<string> { };
            List<float> paddings = new List<float> { };
            while (true)
            {
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
                    if (linewidth + wordwidth > w || (non_start_chars.Contains(nextword) && linewidth + doublesize.Width > w) || (non_end_chars.Contains(nextword) && linewidth + doublesize.Width < w && linewidth + doublesize.Width + size > w))
                    {
                        lines.Add(line);
                        paddings.Add(w - linewidth);
                        line = "";
                        linewidth = 0;
                    }
                    line += word;
                    linewidth = linewidth + wordwidth;
                    pos++;
                }
                if (linewidth > 0)
                {
                    lines.Add(line);
                    paddings.Add(0);
                }
                if (lines.Count * (size + GetLineSpacing(size)) <= h - GetLineSpacing(size))
                    break;
                size -= 0.5f;
                font = new Font(txtFont.Name, size, txtFont.Style, txtFont.Unit);
                lines.Clear();
                paddings.Clear();
            }
            float dx = x;
            float dy = y;
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
                dx = x;
                dy += size + GetLineSpacing(size);
            }
        }

        public static Bitmap Zoom(Bitmap sourceBitmap, int newWidth, int newHeight)
        {
            if (sourceBitmap != null)
            {
                Bitmap b = new Bitmap(newWidth, newHeight);
                Graphics graphics = Graphics.FromImage(b);
                //合成：高质量，低速度
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                //去除锯齿
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                //偏移：高质量，低速度
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //插补算法
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                Rectangle newRect = new Rectangle(0, 0, newWidth, newHeight);
                Rectangle srcRect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
                graphics.DrawImage(sourceBitmap, newRect, srcRect, GraphicsUnit.Pixel);
                graphics.Dispose();
                return b;
            }
            return sourceBitmap;
        }

    }
}

