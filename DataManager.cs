namespace ImgGen
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Drawing.Drawing2D;

    public class DataManager
    {
        private static Bitmap[] bAttributes = new Bitmap[8];
        private static Bitmap[] bStar = new Bitmap[2];
        private static Bitmap[] bTemplates = new Bitmap[15];
        private static Bitmap[] bType = new Bitmap[9];
        private static Bitmap[] bLinkMarkers1 = new Bitmap[9];
        private static Bitmap[] bLinkMarkers2 = new Bitmap[9];

        private static Dictionary<int, Data> cardDatas = new Dictionary<int, Data>();
        private static Dictionary<int, Bitmap> cardImages = new Dictionary<int, Bitmap>();

        private static SQLiteConnection conn;
        private static object locker = new object();

        private static Font nameFont;
        private static Font numFont;
        private static Font linkFont;
        private static Font txtFont;
        private static Font typeFont;
        private static Font scaleFontNormal;
        private static Font scaleFontSmall;
        private static SolidBrush nameBrush = new SolidBrush(Color.FromArgb(64, 64, 0));
        private static SolidBrush typeBrush = new SolidBrush(Color.FromArgb(32, 32, 32));
        private static SolidBrush textBrush = new SolidBrush(Color.FromArgb(64, 64, 64));

        private static string regex_monster = @"[果|介|述|報]】\n([\S\s]*)";
        private static string regex_pendulum = @"】[\s\S]*?\n([\S\s]*?)\n【";

        private static string xyzString = "超量";
        private static string fontName = "文泉驿微米黑";
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
            desc = Regex.Replace(desc, @"(?<=。)([\n\s]+)(?=[①②③④⑤⑥⑦⑧⑨⑩●])", "");
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
            numFont = new Font(fontName, 12, FontStyle.Regular, GraphicsUnit.Pixel);
            linkFont = new Font(fontName, 12, FontStyle.Bold, GraphicsUnit.Pixel);
            nameFont = new Font(fontName, 24, GraphicsUnit.Pixel);
            typeFont = new Font(fontName, 12, FontStyle.Regular, GraphicsUnit.Pixel);
            txtFont = new Font(fontName, 10, GraphicsUnit.Pixel);
            scaleFontNormal = new Font(fontName, 24, GraphicsUnit.Pixel);
            scaleFontSmall = new Font(fontName, 20, GraphicsUnit.Pixel);
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
                if (i == 5) continue;
                bLinkMarkers1[i - 1] = new Bitmap("./textures/link_marker_off_" + i + ".png");
                bLinkMarkers2[i - 1] = new Bitmap("./textures/link_marker_on_" + i + ".png");
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
            Bitmap bitmap;
            SizeF ef;
            int nWidth;
            if (data.isType(Type.TYPE_SPELL))
            {
                bitmap = new Bitmap(bTemplates[0]);
            }
            else if (data.isType(Type.TYPE_TRAP))
            {
                bitmap = new Bitmap(bTemplates[1]);
            }
            else if (data.isType(Type.TYPE_PENDULUM))
            {
                if (data.isType(Type.TYPE_SYNCHRO))
                {
                    bitmap = new Bitmap(bTemplates[12]);
                }
                else if (data.isType(Type.TYPE_XYZ))
                {
                    bitmap = new Bitmap(bTemplates[9]);
                }
                else if (data.isType(Type.TYPE_FUSION))
                {
                    bitmap = new Bitmap(bTemplates[13]);
                }
                else if (data.isType(Type.TYPE_EFFECT))
                {
                    bitmap = new Bitmap(bTemplates[10]);
                }
                else //pnormal
                {
                    bitmap = new Bitmap(bTemplates[11]);
                }
            }
            else if (data.isType(Type.TYPE_LINK))
            {
                bitmap = new Bitmap(bTemplates[14]);
            }
            else if (data.isType(Type.TYPE_SYNCHRO))
            {
                bitmap = new Bitmap(bTemplates[2]);
            }
            else if (data.isType(Type.TYPE_XYZ))
            {
                bitmap = new Bitmap(bTemplates[3]);
            }
            else if (data.isType(Type.TYPE_FUSION))
            {
                bitmap = new Bitmap(bTemplates[4]);
            }
            else if (data.isType(Type.TYPE_RITUAL))
            {
                bitmap = new Bitmap(bTemplates[5]);
            }
            else if (data.isType(Type.TYPE_TOKEN))
            {
                bitmap = new Bitmap(bTemplates[6]);
            }
            else if (data.isType(Type.TYPE_EFFECT))
            {
                bitmap = new Bitmap(bTemplates[7]);
            }
            else //normal
            {
                bitmap = new Bitmap(bTemplates[8]);
            }
            Graphics graphics = Graphics.FromImage(bitmap);
            text.text = GetStandardText(text.text);
            if (data.isType(Type.TYPE_MONSTER))
            {
                if (!zeroStarCards.Contains(data.code))
                {
                    int nStar;
                    if (data.isType(Type.TYPE_XYZ))
                    {
                        for (nStar = 0; nStar < (data.level & 0xff); nStar++)
                        {
                            graphics.DrawImage(bStar[1], (int)35 + (22.5f * nStar), 60, 20, 20);
                        }
                    }
                    else if (!data.isType(Type.TYPE_LINK))
                    {
                        for (nStar = 0; nStar < (data.level & 0xff); nStar++)
                        {
                            graphics.DrawImage(bStar[0], (int)282 - (22.5f * nStar), 60, 20, 20);
                        }
                    }
                }

                int nAttr;
                if (data.attribute == Attribute.ATTRIBUTE_EARTH) nAttr = 0;
                else if (data.attribute == Attribute.ATTRIBUTE_WATER) nAttr = 1;
                else if (data.attribute == Attribute.ATTRIBUTE_FIRE) nAttr = 2;
                else if (data.attribute == Attribute.ATTRIBUTE_WIND) nAttr = 3;
                else if (data.attribute == Attribute.ATTRIBUTE_LIGHT) nAttr = 4;
                else if (data.attribute == Attribute.ATTRIBUTE_DARK) nAttr = 5;
                else nAttr = 6;
                graphics.DrawImage(bAttributes[nAttr], 280, 22, 32, 32);

                if (data.attack >= 0)
                {
                    graphics.DrawString(data.attack.ToString(), numFont, Brushes.Black, 210, 447);
                }
                else
                {
                    graphics.DrawString("?", numFont, textBrush, 210, 447);
                }

                if (data.isType(Type.TYPE_LINK))
                {
                    graphics.DrawString(data.level.ToString(), linkFont, Brushes.Black, 296, 446);
                }
                else
                {
                    if (data.defence >= 0)
                    {
                        graphics.DrawString(data.defence.ToString(), numFont, Brushes.Black, 277, 447);
                    }
                    else
                    {
                        graphics.DrawString("?", numFont, textBrush, 277, 447);
                    }
                }

                string type_string = GetTypeString(data);
                float tWidth = graphics.MeasureString(type_string, typeFont).Width;
                float sx1 = 1f;
                if (tWidth > 280f)
                {
                    sx1 *= 280f / tWidth;
                }
                graphics.ScaleTransform(sx1, 1f);
                graphics.DrawString(type_string, typeFont, typeBrush, 19, 369);
                graphics.ResetTransform();

                string monster_effect = text.text;
                if (data.isType(Type.TYPE_PENDULUM))
                {
                    monster_effect = GetPendulumDesc(text.text, regex_monster);
                }
                nWidth = 288;
                ef = graphics.MeasureString(monster_effect, txtFont, nWidth);
                while (ef.Height > 60 * nWidth / 288f)
                {
                    nWidth += 3;
                    ef = graphics.MeasureString(monster_effect, txtFont, nWidth);
                }
                graphics.TranslateTransform(23f, 385f);
                graphics.ScaleTransform(288f / nWidth, 288f / nWidth);
                graphics.DrawString(monster_effect, txtFont, textBrush, new RectangleF(0f, 0f, ef.Width, ef.Height));
                graphics.ResetTransform();

                if (data.isType(Type.TYPE_PENDULUM))
                {
                    int lscale = (data.level >> 0x18) & 0xff;
                    int rscale = (data.level >> 0x10) & 0xff;
                    if (lscale > 9)
                    {
                        graphics.DrawString(lscale.ToString(), scaleFontSmall, Brushes.Black, 19f, 336f);
                    }
                    else
                    {
                        graphics.DrawString(lscale.ToString(), scaleFontNormal, Brushes.Black, 24f, 333f);
                    }
                    if (rscale > 9)
                    {
                        graphics.DrawString(rscale.ToString(), scaleFontSmall, Brushes.Black, 287f, 336f);
                    }
                    else
                    {
                        graphics.DrawString(rscale.ToString(), scaleFontNormal, Brushes.Black, 291f, 333f);
                    }
                    int nWidthP;
                    SizeF pf;
                    string pendulum_effect = GetPendulumDesc(text.text, regex_pendulum);
                    nWidthP = 232;
                    pf = graphics.MeasureString(pendulum_effect, txtFont, nWidthP);
                    while (pf.Height > 52 * nWidthP / 232f)
                    {
                        nWidthP += 1;
                        pf = graphics.MeasureString(pendulum_effect, txtFont, nWidthP);
                    }
                    graphics.TranslateTransform(51f, 311f);
                    graphics.ScaleTransform(232f / nWidthP, 232f / nWidthP);
                    graphics.DrawString(pendulum_effect, txtFont, textBrush, new RectangleF(0f, 0f, pf.Width, pf.Height));
                    graphics.ResetTransform();
                }
            }
            else
            {
                if (data.isType(Type.TYPE_SPELL))
                {
                    if (data.type == Type.TYPE_SPELL)
                    {
                        graphics.DrawImage(bType[0], 204, 60, 96, 19);
                    }
                    else
                    {
                        int nType = 0;
                        if (data.isType(Type.TYPE_QUICKPLAY)) nType = 1;
                        if (data.isType(Type.TYPE_CONTINUOUS)) nType = 2;
                        if (data.isType(Type.TYPE_EQUIP)) nType = 3;
                        if (data.isType(Type.TYPE_FIELD)) nType = 4;
                        if (data.isType(Type.TYPE_RITUAL)) nType = 5;
                        graphics.DrawImage(bType[nType], 192, 60, 108, 19);
                    }
                }
                else if (data.isType(Type.TYPE_TRAP))
                {
                    if (data.type == Type.TYPE_TRAP)
                    {
                        graphics.DrawImage(bType[6], 221, 60, 80, 19);
                    }
                    else
                    {
                        int nType = 6;
                        if (data.isType(Type.TYPE_CONTINUOUS)) nType = 7;
                        if (data.isType(Type.TYPE_COUNTER)) nType = 8;
                        graphics.DrawImage(bType[nType], 209, 60, 91, 19);
                    }
                }
                nWidth = 288;
                ef = graphics.MeasureString(text.text, txtFont, nWidth);
                while (ef.Height > 80 * nWidth / 288f)
                {
                    nWidth += 3;
                    ef = graphics.MeasureString(text.text, txtFont, nWidth);
                }
                graphics.TranslateTransform(24f, 370f);
                graphics.ScaleTransform(288f / nWidth, 288f / nWidth);
                graphics.DrawString(text.text, txtFont, textBrush, new RectangleF(0f, 0f, ef.Width, ef.Height));
                graphics.ResetTransform();
            }
            try
            {
                Bitmap image = new Bitmap("./pico/" + code.ToString() + ".jpg");
                graphics.CompositingMode = CompositingMode.SourceOver;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                if (data.isType(Type.TYPE_PENDULUM))
                {
                    float ar = image.Width / image.Height;
                    if ((ar >= 1.3) && (ar <= 1.4))
                        graphics.DrawImage(image, 22, 88, 292, 217);
                    else
                        graphics.DrawImage(image, new Rectangle(22, 88, 292, 217), new Rectangle(0, 0, image.Width, image.Width * 217 / 292), GraphicsUnit.Pixel);
                }
                else
                {
                    graphics.DrawImage(image, 40, 90, 256, 256);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error prasing {0} {1}", data.code, e);
            }
            if (data.isType(Type.TYPE_LINK))
            {
                LinkMarker lm = (LinkMarker)data.defence;
                if ((lm & LinkMarker.LINK_MARKER_BOTTOM_LEFT) == 0)
                    graphics.DrawImage(bLinkMarkers1[0], 26, 326, 34, 34);
                else
                    graphics.DrawImage(bLinkMarkers2[0], 26, 326, 34, 34);
                if ((lm & LinkMarker.LINK_MARKER_BOTTOM) == 0)
                    graphics.DrawImage(bLinkMarkers1[1], 139, 343, 62, 23);
                else
                    graphics.DrawImage(bLinkMarkers2[1], 139, 343, 62, 23);
                if ((lm & LinkMarker.LINK_MARKER_BOTTOM_RIGHT) == 0)
                    graphics.DrawImage(bLinkMarkers1[2], 277, 326, 35, 34);
                else
                    graphics.DrawImage(bLinkMarkers2[2], 277, 326, 35, 34);
                if ((lm & LinkMarker.LINK_MARKER_LEFT) == 0)
                    graphics.DrawImage(bLinkMarkers1[3], 20, 187, 23, 62);
                else
                    graphics.DrawImage(bLinkMarkers2[3], 20, 187, 23, 62);
                if ((lm & LinkMarker.LINK_MARKER_RIGHT) == 0)
                    graphics.DrawImage(bLinkMarkers1[5], 294, 187, 23, 62);
                else
                    graphics.DrawImage(bLinkMarkers2[5], 294, 187, 23, 62);
                if ((lm & LinkMarker.LINK_MARKER_TOP_LEFT) == 0)
                    graphics.DrawImage(bLinkMarkers1[6], 26, 75, 35, 34);
                else
                    graphics.DrawImage(bLinkMarkers2[6], 26, 75, 35, 34);
                if ((lm & LinkMarker.LINK_MARKER_TOP) == 0)
                    graphics.DrawImage(bLinkMarkers1[7], 138, 69, 63, 23);
                else
                    graphics.DrawImage(bLinkMarkers2[7], 138, 69, 63, 23);
                if ((lm & LinkMarker.LINK_MARKER_TOP_RIGHT) == 0)
                    graphics.DrawImage(bLinkMarkers1[8], 278, 75, 34, 34);
                else
                    graphics.DrawImage(bLinkMarkers2[8], 278, 75, 34, 34);
            }
            string nametext = text.name.Replace('\x00b7', '・');
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            float width = graphics.MeasureString(nametext, nameFont).Width;
            float sx = 1f;
            if (width > 265f)
            {
                sx *= 265f / width;
            }
            graphics.TranslateTransform(21f, 23f);
            graphics.ScaleTransform(sx, 1f);
            graphics.DrawString(nametext, nameFont, nameBrush, 0f, 0f);
            graphics.DrawString(nametext, nameFont, Brushes.Gold, 1f, 1f);
            graphics.ResetTransform();
            return bitmap;
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

