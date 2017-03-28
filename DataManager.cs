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
        private static Bitmap[] bTemplates = new Bitmap[16];
        private static Bitmap[] bType = new Bitmap[15];
        private static Bitmap[] bLinkMarkers1 = new Bitmap[9];
        private static Bitmap[] bLinkMarkers2 = new Bitmap[9];
        private static Dictionary<int, Data> cardDatas = new Dictionary<int, Data>();
        private static Dictionary<int, Bitmap> cardImages = new Dictionary<int, Bitmap>();
        private static Dictionary<int, Text> cardTexts = new Dictionary<int, Text>();
        private static SQLiteConnection conn;
        private static Dictionary<int, string> ctStrings = new Dictionary<int, string>();
        private static object locker = new object();
        private static SolidBrush nameBrush = new SolidBrush(Color.FromArgb(64, 64, 0));
        private static Font nameFont;
        private static Font numFont;
        private static Font linkFont;
        private static Dictionary<int, string> sysStrings = new Dictionary<int, string>();
        private static SolidBrush textBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
        private static Font txtFont;
        private static SolidBrush typeBrush = new SolidBrush(Color.FromArgb(32, 32, 32));
        private static Font typeFont;
        private static Dictionary<int, string> winStrings = new Dictionary<int, string>();

        private static string regex_monster = @"[果|介|述|報]】\n([\S\s]*)";
        private static string regex_pendulum = @"】[\s\S]*?\n([\S\s]*?)\n【";
        private static string xyzString = "超量";

        public static Data GetCardData(int code)
        {
            if (!cardDatas.ContainsKey(code))
            {
                LoadCard(code);
            }
            return cardDatas[code];
        }

        public static string GetCardDescription(int code)
        {
            if (!cardTexts.ContainsKey(code))
            {
                LoadCard(code);
            }
            return cardTexts[code].text;
        }

        public static string GetCardName(int code)
        {
            if (!cardTexts.ContainsKey(code))
            {
                LoadCard(code);
            }
            return cardTexts[code].name;
        }

        public static string GetCardString(int desc)
        {
            int key = (desc >> 4) & 0xfffffff;
            int index = desc & 15;
            if (!cardTexts.ContainsKey(key))
            {
                LoadCard(key);
            }
            return cardTexts[key].desc[index];
        }

        public static string GetCounterName(int code)
        {
            if (ctStrings.ContainsKey(code))
            {
                return ctStrings[code];
            }
            return "";
        }

        public static Bitmap GetImage(int code)
        {
            if (!cardImages.ContainsKey(code))
            {
                LoadCard(code);
            }
            return cardImages[code];
        }

        public static string GetSystemString(int code)
        {
            if (sysStrings.ContainsKey(code))
            {
                return sysStrings[code];
            }
            return "";
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
            }
            if (data.isType(Type.TYPE_FUSION))
            {
                str = str + "／融合";
            }
            if (data.isType(Type.TYPE_SYNCHRO))
            {
                str = str + "／同调";
            }
            if (data.isType(Type.TYPE_LINK))
            {
                str = str + "／连接";
            }
            else if (data.isType(Type.TYPE_XYZ))
            {
                str = str + "／" + xyzString;
            }
            if (data.isType(Type.TYPE_RITUAL))
            {
                str = str + "／仪式";
            }
            if (data.isType(Type.TYPE_SPSUMMON))
            {
                str = str + "／特殊召唤";
            }
            if (data.isType(Type.TYPE_PENDULUM))
            {
                str = str + "／灵摆";
            }
            if (data.isType(Type.TYPE_SPIRIT))
            {
                str = str + "／灵魂";
            }
            else if (data.isType(Type.TYPE_DUAL))
            {
                str = str + "／二重";
            }
            else if (data.isType(Type.TYPE_UNION))
            {
                str = str + "／同盟";
            }
            else if (data.isType(Type.TYPE_FLIP))
            {
                str = str + "／反转";
            }
            else if (data.isType(Type.TYPE_TOON))
            {
                str = str + "／卡通";
            }
            if (data.isType(Type.TYPE_TUNER))
            {
                str = str + "／调整";
            }
            if (data.isType(Type.TYPE_EFFECT))
            {
                str = str + "／效果";
            }
            if (data.isType(Type.TYPE_NORMAL))
            {
                str = str + "／通常";
            }
            return (str + "】");
        }

        public static void InitialDatas(string dbPath = "../cards.cdb", string xyz = "超量")
        {
            xyzString = xyz;
            conn = new SQLiteConnection("Data Source=" + dbPath);
            numFont = new Font("Arial", 5.5f);
            linkFont = new Font("Tohoma", 5.5f, FontStyle.Bold);
            nameFont = new Font("文泉驿微米黑", 10f);
            typeFont = new Font("文泉驿微米黑", 6f);
            txtFont = new Font("文泉驿微米黑", 5f);
            bTemplates[0] = new Bitmap("./textures/card_spell.jpg");
            bTemplates[1] = new Bitmap("./textures/card_trap.jpg");
            bTemplates[2] = new Bitmap("./textures/card_synchro.jpg");
            bTemplates[3] = new Bitmap("./textures/card_xyz.jpg");
            bTemplates[4] = new Bitmap("./textures/card_fusion.jpg");
            bTemplates[5] = new Bitmap("./textures/card_ritual.jpg");
            bTemplates[6] = new Bitmap("./textures/card_token.jpg");
            bTemplates[7] = new Bitmap("./textures/card_effect.jpg");
            bTemplates[8] = new Bitmap("./textures/card_normal.jpg");
            bTemplates[9] = new Bitmap("./textures/card_pxyz.jpg");
            bTemplates[10] = new Bitmap("./textures/card_peffect.jpg");
            bTemplates[11] = new Bitmap("./textures/card_pnormal.jpg");
            bTemplates[12] = new Bitmap("./textures/card_psynchro.jpg");
            bTemplates[13] = new Bitmap("./textures/card_pfusion.jpg");
            bTemplates[14] = new Bitmap("./textures/card_zarc.jpg");
            bTemplates[15] = new Bitmap("./textures/card_link.jpg");
            bAttributes[0] = new Bitmap("./textures/att_earth.png");
            bAttributes[1] = new Bitmap("./textures/att_water.png");
            bAttributes[2] = new Bitmap("./textures/att_fire.png");
            bAttributes[3] = new Bitmap("./textures/att_wind.png");
            bAttributes[4] = new Bitmap("./textures/att_light.png");
            bAttributes[5] = new Bitmap("./textures/att_dark.png");
            bAttributes[6] = new Bitmap("./textures/att_devine.png");
            bStar[0] = new Bitmap("./textures/star.png");
            bStar[1] = new Bitmap("./textures/starb.png");
            bType[0] = new Bitmap("./textures/spell1.png");
            bType[1] = new Bitmap("./textures/spell2.png");
            bType[2] = new Bitmap("./textures/trap1.png");
            bType[3] = new Bitmap("./textures/trap2.png");
            bType[4] = new Bitmap("./textures/type_continuous.png");
            bType[5] = new Bitmap("./textures/type_counter.png");
            bType[6] = new Bitmap("./textures/type_equip.png");
            bType[7] = new Bitmap("./textures/type_field.png");
            bType[8] = new Bitmap("./textures/type_quickplay.png");
            bType[9] = new Bitmap("./textures/type_ritual.png");
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
                int num2;
                if (cardDatas.ContainsKey(code))
                {
                    return 0;
                }
                Data data = new Data();
                Text text = new Text();
                data.code = code;
                text.desc = new string[0x10];
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
                        num2 = 0;
                        while (num2 < 0x10)
                        {
                            text.desc[num2] = reader.GetString(3 + num2);
                            num2++;
                        }
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
                cardTexts.Add(code, text);
                if (!cardImages.ContainsKey(code))
                {
                    Bitmap bitmap;
                    SizeF ef;
                    int num4;
                    if (((int)data.type & 0x1802040) == 0x1802040)
                    {
                        bitmap = new Bitmap(bTemplates[14]);
                    }
                    else if (data.isType(Type.TYPE_SPELL))
                    {
                        bitmap = new Bitmap(bTemplates[0]);
                    }
                    else if (data.isType(Type.TYPE_TRAP))
                    {
                        bitmap = new Bitmap(bTemplates[1]);
                    }
                    else if (data.isType(Type.TYPE_SYNCHRO))
                    {
                        if (data.isType(Type.TYPE_PENDULUM))
                        {
                            bitmap = new Bitmap(bTemplates[12]);
                        }
                        else
                        {
                            bitmap = new Bitmap(bTemplates[2]);
                        }
                    }
                    else if (data.isType(Type.TYPE_LINK))
                    {
                        bitmap = new Bitmap(bTemplates[15]);
                    }
                    else if (data.isType(Type.TYPE_XYZ))
                    {
                        if (data.isType(Type.TYPE_PENDULUM))
                        {
                            bitmap = new Bitmap(bTemplates[9]);
                        }
                        else
                        {
                            bitmap = new Bitmap(bTemplates[3]);
                        }
                    }
                    else if (data.isType(Type.TYPE_FUSION))
                    {
                        if (data.isType(Type.TYPE_PENDULUM))
                        {
                            bitmap = new Bitmap(bTemplates[13]);
                        }
                        else
                        {
                            bitmap = new Bitmap(bTemplates[4]);
                        }
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
                        if (data.isType(Type.TYPE_PENDULUM))
                        {
                            bitmap = new Bitmap(bTemplates[10]);
                        }
                        else
                        {
                            bitmap = new Bitmap(bTemplates[7]);
                        }
                    }
                    else
                    {
                        if (data.isType(Type.TYPE_PENDULUM))
                        {
                            bitmap = new Bitmap(bTemplates[11]);
                        }
                        else
                        {
                            bitmap = new Bitmap(bTemplates[8]);
                        }
                    }
                    Graphics graphics = Graphics.FromImage(bitmap);
                    text.text = tosbc(text.text);
                    if (data.isType(Type.TYPE_MONSTER))
                    {
                        int x = 144;
                        int y = 15;
                        if (!data.isType(Type.TYPE_LINK) && (!data.isType(Type.TYPE_XYZ) || ((int)data.type & 0x1802040) == 0x1802040))
                        {
                            if (!data.isType(Type.TYPE_PENDULUM))
                            {
                                for (num2 = 0; num2 < (data.level & 0xff); num2++)
                                {
                                    graphics.DrawImage(bStar[0], 149 - (12 * num2), 37, 11, 11);
                                }
                            }
                            else
                            {
                                for (num2 = 0; num2 < (data.level & 0xff); num2++)
                                {
                                    graphics.DrawImage(bStar[0], 149 - (12 * num2), 35, 11, 11);
                                }
                                y = 12;
                            }
                        }
                        if (data.isType(Type.TYPE_LINK))
                        {
                            x = 147;
                            y = 11;
                        }
                        else if (data.isType(Type.TYPE_XYZ))
                        {
                            for (num2 = 0; num2 < (data.level & 0xff); num2++)
                            {
                                graphics.DrawImage(bStar[1], 17 + (12 * num2), 35, 11, 11);
                            }
                            y = 12;
                        }
                        if (data.attribute == Attribute.ATTRIBUTE_EARTH)
                        {
                            graphics.DrawImage(bAttributes[0], x, y, 18, 18);
                        }
                        else if (data.attribute == Attribute.ATTRIBUTE_WATER)
                        {
                            graphics.DrawImage(bAttributes[1], x, y, 18, 18);
                        }
                        else if (data.attribute == Attribute.ATTRIBUTE_FIRE)
                        {
                            graphics.DrawImage(bAttributes[2], x, y, 18, 18);
                        }
                        else if (data.attribute == Attribute.ATTRIBUTE_WIND)
                        {
                            graphics.DrawImage(bAttributes[3], x, y, 18, 18);
                        }
                        else if (data.attribute == Attribute.ATTRIBUTE_LIGHT)
                        {
                            graphics.DrawImage(bAttributes[4], x, y, 18, 18);
                        }
                        else if (data.attribute == Attribute.ATTRIBUTE_DARK)
                        {
                            graphics.DrawImage(bAttributes[5], x, y, 18, 18);
                        }
                        else if (data.attribute == Attribute.ATTRIBUTE_DEVINE)
                        {
                            graphics.DrawImage(bAttributes[6], x, y, 18, 18);
                        }
                        if (data.isType(Type.TYPE_LINK))
                        {
                            if (data.attack >= 0)
                            {
                                graphics.DrawString(data.attack.ToString(), numFont, Brushes.Black, 110f, 231f);
                            }
                            else
                            {
                                graphics.DrawString("?", numFont, textBrush, 110f, 231f);
                            }
                            graphics.DrawString(data.level.ToString(), linkFont, Brushes.Black, 156f, 231f);
                        }
                        else if (!data.isType(Type.TYPE_PENDULUM))
                        {
                            if (!data.isType(Type.TYPE_XYZ))
                            {
                                if (data.attack >= 0)
                                {
                                    graphics.DrawString(data.attack.ToString(), numFont, Brushes.Black, 105f, 231f);
                                }
                                else
                                {
                                    graphics.DrawString("?", numFont, textBrush, 115f, 231f);
                                }
                                if (data.defence >= 0)
                                {
                                    graphics.DrawString(data.defence.ToString(), numFont, Brushes.Black, 142f, 231f);
                                }
                                else
                                {
                                    graphics.DrawString("?", numFont, textBrush, 152f, 231f);
                                }
                            }
                            else
                            {
                                if (data.attack >= 0)
                                {
                                    graphics.DrawString(data.attack.ToString(), numFont, Brushes.Black, 109f, 231f);
                                }
                                else
                                {
                                    graphics.DrawString("?", numFont, textBrush, 119f, 231f);
                                }
                                if (data.defence >= 0)
                                {
                                    graphics.DrawString(data.defence.ToString(), numFont, Brushes.Black, 145f, 231f);
                                }
                                else
                                {
                                    graphics.DrawString("?", numFont, textBrush, 155f, 231f);
                                }
                            }
                        }
                        else
                        {
                            if (data.attack >= 0)
                            {
                                graphics.DrawString(data.attack.ToString(), numFont, Brushes.Black, 105f, 231f);
                            }
                            else
                            {
                                graphics.DrawString("?", numFont, textBrush, 115f, 231f);
                            }
                            if (data.defence >= 0)
                            {
                                graphics.DrawString(data.defence.ToString(), numFont, Brushes.Black, 142f, 231f);
                            }
                            else
                            {
                                graphics.DrawString("?", numFont, textBrush, 152f, 231f);
                            }
                        }
                        if (data.isType(Type.TYPE_LINK))
                        {
                            graphics.DrawString(GetTypeString(data), typeFont, typeBrush, 10f, 192f);
                            ef = graphics.MeasureString(text.text, txtFont, 150);
                            num4 = 150;
                            while (ef.Height > 28 * num4 / 150f)
                            {
                                num4 += 3;
                                ef = graphics.MeasureString(text.text, txtFont, num4);
                            }
                            graphics.TranslateTransform(14f, 202f);
                            graphics.ScaleTransform(150f / num4, 150f / num4);
                            graphics.DrawString(text.text, txtFont, textBrush, new RectangleF(0f, 0f, ef.Width, ef.Height));
                            graphics.ResetTransform();
                        }
                        else if (data.isType(Type.TYPE_PENDULUM))
                        {
                            string type_string = GetTypeString(data);
                            float width1 = graphics.MeasureString(type_string, typeFont).Width;
                            float sx1 = 1f;
                            if (width1 > 150f)
                            {
                                sx1 *= 150f / width1;
                            }
                            graphics.ScaleTransform(sx1, 1f);
                            graphics.DrawString(type_string, typeFont, typeBrush, 12f, 192f);
                            graphics.ResetTransform();
                            string monster_effect = GetDesc(text.text, regex_monster);
                            string pendulum_effect = GetDesc(text.text, regex_pendulum);
                            int lscale = (data.level >> 0x18) & 0xff;
                            int rscale = (data.level >> 0x10) & 0xff;
                            if (lscale > 9)
                            {
                                graphics.DrawString(lscale.ToString(), new Font("文泉驿微米黑", 7f), Brushes.Black, 13f, 174f);
                            }
                            else
                            {
                                graphics.DrawString(lscale.ToString(), new Font("文泉驿微米黑", 8f), Brushes.Black, 16f, 174f);
                            }
                            if (rscale > 9)
                            {
                                graphics.DrawString(rscale.ToString(), new Font("文泉驿微米黑", 7f), Brushes.Black, 150f, 174f);
                            }
                            else
                            {
                                graphics.DrawString(rscale.ToString(), new Font("文泉驿微米黑", 8f), Brushes.Black, 151f, 174f);
                            }
                            ef = graphics.MeasureString(monster_effect, txtFont, 145);
                            num4 = 145;
                            while (ef.Height > 28 * num4 / 145f)
                            {
                                num4 += 3;
                                ef = graphics.MeasureString(monster_effect, txtFont, num4);
                            }
                            graphics.TranslateTransform(16f, 202f);
                            graphics.ScaleTransform(145f / num4, 145f / num4);
                            graphics.DrawString(monster_effect, txtFont, textBrush, new RectangleF(0f, 0f, ef.Width, ef.Height));
                            graphics.ResetTransform();
                            int num5;
                            SizeF pf;
                            pf = graphics.MeasureString(pendulum_effect, txtFont, 119);
                            num5 = 119;
                            while (pf.Height > 24 * num5 / 119f)
                            {
                                num5 += 1;
                                pf = graphics.MeasureString(pendulum_effect, txtFont, num5);
                            }
                            graphics.TranslateTransform(29f, 162f);
                            graphics.ScaleTransform(119f / num5, 119f / num5);
                            graphics.DrawString(pendulum_effect, txtFont, textBrush, new RectangleF(0f, 0f, pf.Width, pf.Height));
                            graphics.ResetTransform();
                        }
                        else if (data.isType(Type.TYPE_XYZ))
                        {
                            graphics.DrawString(GetTypeString(data), typeFont, typeBrush, 12f, 192f);
                            ef = graphics.MeasureString(text.text, txtFont, 145);
                            num4 = 145;
                            while (ef.Height > 28 * num4 / 145f)
                            {
                                num4 += 3;
                                ef = graphics.MeasureString(text.text, txtFont, num4);
                            }
                            graphics.TranslateTransform(16f, 202f);
                            graphics.ScaleTransform(145f / num4, 145f / num4);
                            graphics.DrawString(text.text, txtFont, textBrush, new RectangleF(0f, 0f, ef.Width, ef.Height));
                            graphics.ResetTransform();
                        }
                        else {
                            graphics.DrawString(GetTypeString(data), typeFont, typeBrush, 13f, 195f);
                            ef = graphics.MeasureString(text.text, txtFont, 145);
                            num4 = 145;
                            while (ef.Height > 25 * num4 / 145f)
                            {
                                num4 += 3;
                                ef = graphics.MeasureString(text.text, txtFont, num4);
                            }
                            graphics.TranslateTransform(16f, 205f);
                            graphics.ScaleTransform(145f / num4, 145f / num4);
                            graphics.DrawString(text.text, txtFont, textBrush, new RectangleF(0f, 0f, ef.Width, ef.Height));
                            graphics.ResetTransform();
                        }
                    }
                    else
                    {
                        if (data.isType(Type.TYPE_SPELL))
                        {
                            if (data.type == Type.TYPE_SPELL)
                            {
                                graphics.DrawImage(bType[0], 101, 37);
                            }
                            else
                            {
                                graphics.DrawImage(bType[1], 92, 37);
                                if (data.isType(Type.TYPE_CONTINUOUS))
                                {
                                    graphics.DrawImage(bType[4], 143, 40);
                                }
                                else if (data.isType(Type.TYPE_EQUIP))
                                {
                                    graphics.DrawImage(bType[6], 143, 40);
                                }
                                else if (data.isType(Type.TYPE_FIELD))
                                {
                                    graphics.DrawImage(bType[7], 143, 40);
                                }
                                else if (data.isType(Type.TYPE_QUICKPLAY))
                                {
                                    graphics.DrawImage(bType[8], 143, 40);
                                }
                                else if (data.isType(Type.TYPE_RITUAL))
                                {
                                    graphics.DrawImage(bType[9], 143, 40);
                                }
                            }
                        }
                        else if (data.isType(Type.TYPE_TRAP))
                        {
                            if (data.type == Type.TYPE_TRAP)
                            {
                                graphics.DrawImage(bType[2], 111, 37);
                            }
                            else
                            {
                                graphics.DrawImage(bType[3], 102, 37);
                                if (data.isType(Type.TYPE_CONTINUOUS))
                                {
                                    graphics.DrawImage(bType[4], 143, 40);
                                }
                                else if (data.isType(Type.TYPE_COUNTER))
                                {
                                    graphics.DrawImage(bType[5], 143, 40);
                                }
                            }
                        }
                        ef = graphics.MeasureString(text.text, txtFont, 145);
                        num4 = 145;
                        while (ef.Height > 40 * num4 / 145f)
                        {
                            num4 += 3;
                            ef = graphics.MeasureString(text.text, txtFont, num4);
                        }
                        graphics.TranslateTransform(16f, 195f);
                        graphics.ScaleTransform(145f / num4, 145f / num4);
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
                        if (data.isType(Type.TYPE_LINK))
                        {
                            graphics.DrawImage(image, 22, 47, 133, 133);
                        }
                        else if (data.isType(Type.TYPE_PENDULUM))
                        {
                            //graphics.DrawImage(image, 15, 50, 147, 109);
                            graphics.DrawImage(image, new Rectangle(15, 50, 147, 109), new Rectangle(0, 0, image.Width, image.Width * 109 / 147), GraphicsUnit.Pixel);
                        }
                        else if (!data.isType(Type.TYPE_XYZ))
                        {
                            graphics.DrawImage(image, 25, 54, 128, 128);
                        }
                        else
                        {
                            graphics.DrawImage(image, 24, 51, 128, 128);
                        }
                    }
                    catch
                    {
                    }
                    if (data.isType(Type.TYPE_LINK))
                    {
                        if ((data.defence & 0x1) == 0)
                            graphics.DrawImage(bLinkMarkers1[0], 14, 169, 18, 17);
                        else
                            graphics.DrawImage(bLinkMarkers2[0], 14, 169, 18, 17);
                        if ((data.defence & 0x2) == 0)
                            graphics.DrawImage(bLinkMarkers1[1], 73, 178, 32, 12);
                        else
                            graphics.DrawImage(bLinkMarkers2[1], 73, 178, 32, 12);
                        if ((data.defence & 0x4) == 0)
                            graphics.DrawImage(bLinkMarkers1[2], 145, 169, 18, 17);
                        else
                            graphics.DrawImage(bLinkMarkers2[2], 145, 169, 18, 17);
                        if ((data.defence & 0x8) == 0)
                            graphics.DrawImage(bLinkMarkers1[3], 11, 97, 12, 32);
                        else
                            graphics.DrawImage(bLinkMarkers2[3], 11, 97, 12, 32);
                        if ((data.defence & 0x20) == 0)
                            graphics.DrawImage(bLinkMarkers1[5], 154, 97, 12, 32);
                        else
                            graphics.DrawImage(bLinkMarkers2[5], 154, 97, 12, 32);
                        if ((data.defence & 0x40) == 0)
                            graphics.DrawImage(bLinkMarkers1[6], 14, 39, 18, 17);
                        else
                            graphics.DrawImage(bLinkMarkers2[6], 14, 39, 18, 17);
                        if ((data.defence & 0x80) == 0)
                            graphics.DrawImage(bLinkMarkers1[7], 73, 36, 32, 12);
                        else
                            graphics.DrawImage(bLinkMarkers2[7], 73, 36, 32, 12);
                        if ((data.defence & 0x100) == 0)
                            graphics.DrawImage(bLinkMarkers1[8], 146, 39, 18, 17);
                        else
                            graphics.DrawImage(bLinkMarkers2[8], 146, 39, 18, 17);
                    }
                    string str3 = text.name.Replace('\x00b7', '・');
                    float width = graphics.MeasureString(str3, nameFont).Width;
                    float sx = 1f;
                    if (width > 130f)
                    {
                        sx *= 130f / width;
                    }
                    if (data.isType(Type.TYPE_LINK))
                    {
                        graphics.TranslateTransform(10f, 13f);
                    }
                    else if (data.isType(Type.TYPE_XYZ))
                    {
                        graphics.TranslateTransform(12f, 13f);
                    }
                    else if (data.isType(Type.TYPE_PENDULUM))
                    {
                        graphics.TranslateTransform(12f, 13f);
                    }
                    else
                    {
                        graphics.TranslateTransform(14f, 17f);
                    }
                    graphics.ScaleTransform(sx, 1f);
                    graphics.DrawString(str3, nameFont, nameBrush, 0f, 0f);
                    graphics.DrawString(str3, nameFont, Brushes.Gold, 1f, 1f);
                    graphics.ResetTransform();
                    cardImages.Add(code, bitmap);
                }
                return 0;
            }
        }

        private static string tosbc(string r)
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

        private static string GetDesc(string cdesc, string regx)
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

