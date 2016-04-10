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
        private static Bitmap[] bType = new Bitmap[15];
        private static Dictionary<int, Data> cardDatas = new Dictionary<int, Data>();
        private static Dictionary<int, Bitmap> cardImages = new Dictionary<int, Bitmap>();
        private static Dictionary<int, Text> cardTexts = new Dictionary<int, Text>();
        private static SQLiteConnection conn = new SQLiteConnection("Data Source=../cards.cdb");
        private static Dictionary<int, string> ctStrings = new Dictionary<int, string>();
        private static object locker = new object();
        private static SolidBrush nameBrush = new SolidBrush(Color.FromArgb(0x40, 0x40, 0));
        private static Font nameFont;
        private static Font numFont;
        private static Dictionary<int, string> sysStrings = new Dictionary<int, string>();
        private static SolidBrush textBrush = new SolidBrush(Color.FromArgb(0x40, 0x40, 0x40));
        private static Font txtFont;
        private static SolidBrush typeBrush = new SolidBrush(Color.FromArgb(0x20, 0x20, 0x20));
        private static Font typeFont;
        private static Dictionary<int, string> winStrings = new Dictionary<int, string>();

		private static string regex_monster = @"[果|介|述|報]】\n([\S\s]*)";
		private static string regex_pendulum = @"】[\s\S]*?\n([\S\s]*?)\n【";

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

        private static string gettypestring(Data dat)
        {
            string str = "【";
            switch (dat.race)
            {
                case 0x10:
                    str = str + "不死族";
                    break;

                case 0x20:
                    str = str + "机械族";
                    break;

                case 0x40:
                    str = str + "水族";
                    break;

                case 1:
                    str = str + "战士族";
                    break;

                case 2:
                    str = str + "魔法师族";
                    break;

                case 4:
                    str = str + "天使族";
                    break;

                case 8:
                    str = str + "恶魔族";
                    break;

                case 0x80:
                    str = str + "炎族";
                    break;

                case 0x100:
                    str = str + "岩石族";
                    break;

                case 0x200:
                    str = str + "鸟兽族";
                    break;

                case 0x400:
                    str = str + "植物族";
                    break;

                case 0x800:
                    str = str + "昆虫族";
                    break;

                case 0x4000:
                    str = str + "兽族";
                    break;

                case 0x8000:
                    str = str + "兽战士族";
                    break;

                case 0x10000:
                    str = str + "恐龙族";
                    break;

                case 0x1000:
                    str = str + "雷族";
                    break;

                case 0x2000:
                    str = str + "龙族";
                    break;

                case 0x20000:
                    str = str + "鱼族";
                    break;

                case 0x40000:
                    str = str + "海龙族";
                    break;

                case 0x80000:
                    str = str + "爬虫类族";
                    break;

                case 0x100000:
                    str = str + "念动力族";
                    break;

                case 0x200000:
                    str = str + "幻神兽族";
                    break;

                case 0x400000:
                    str = str + "创造神族";
                    break;

                case 0x800000:
                    str = str + "幻龙族";
                    break;
            }
            if ((dat.type & 0x8020c0) != 0)
            {
                if ((dat.type & 0x800000) != 0)
                {
                    str = str + "／超量";
                }
                else if ((dat.type & 0x2000) != 0)
                {
                    str = str + "／同调";
                }
                else if ((dat.type & 0x40) != 0)
                {
                    str = str + "／融合";
                }
                else if ((dat.type & 0x80) != 0)
                {
                    str = str + "／仪式";
                }
            }
            if ((dat.type & 0x1000000) != 0)
            {
                str = str + "／灵摆";
            }
            if ((dat.type & 0x200) != 0)
            {
                str = str + "／灵魂";
            }
            else if ((dat.type & 0x800) != 0)
            {
                str = str + "／二重";
            }
            else if ((dat.type & 0x400) != 0)
            {
                str = str + "／同盟";
            }
            else if ((dat.type & 0x200000) != 0)
            {
                str = str + "／反转";
            }
            else if ((dat.type & 0x400000) != 0)
            {
                str = str + "／卡通";
            }
            if ((dat.type & 0x1000) != 0)
            {
                str = str + "／调整";
            }
            if ((dat.type & 0x20) != 0)
            {
                str = str + "／效果";
            }
            return (str + "】");
        }

        public static void InitialDatas()
        {
            numFont = new Font("Arial", 5.5f);
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
                        data.type = reader.GetInt32(4);
                        data.attack = reader.GetInt32(5);
                        data.defence = reader.GetInt32(6);
                        data.level = reader.GetInt32(7);
                        data.race = reader.GetInt32(8);
                        data.attribute = reader.GetInt32(9);
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
                    Bitmap bitmap, bitmap_answer;
                    bitmap_answer = new Bitmap(177, 254);
                    SizeF ef;
                    int num4;
                    if ((data.type & 2) != 0)
                    {
                        bitmap = new Bitmap(bTemplates[0]);
                    }
                    else if ((data.type & 4) != 0)
                    {
                        bitmap = new Bitmap(bTemplates[1]);
                    }
                    else if ((data.type & 0x2000) != 0)
                    {
                        if ((data.type & 0x1000000) != 0)
                        {
                            bitmap = new Bitmap(bTemplates[12]);
                        }
                        else
                        {
                            bitmap = new Bitmap(bTemplates[2]);
                        }
                    }
                    else if ((data.type & 0x800000) != 0)
                    {
                        if ((data.type & 0x1000000) != 0)
                        {
                            bitmap = new Bitmap(bTemplates[9]);
                        }
                        else
                        {
                            bitmap = new Bitmap(bTemplates[3]);
                        }
                    }
                    else if ((data.type & 0x40) != 0)
                    {
                        bitmap = new Bitmap(bTemplates[4]);
                    }
                    else if ((data.type & 0x80) != 0)
                    {
                        bitmap = new Bitmap(bTemplates[5]);
                    }
                    else if ((data.type & 0x4000) != 0)
                    {
                        bitmap = new Bitmap(bTemplates[6]);
                    }
                    else if ((data.type & 0x20) != 0)
                    {
                        if ((data.type & 0x1000000) != 0)
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
                        if ((data.type & 0x1000000) != 0)
                        {
                            bitmap = new Bitmap(bTemplates[11]);
                        }
                        else
                        {
                            bitmap = new Bitmap(bTemplates[8]);
                        }
                    }
                    Graphics graphics = Graphics.FromImage(bitmap_answer);
                    grpahics.DrawImage(bitmap, 0, 0);
                    text.text = tosbc(text.text);
                    if ((data.type & 1) != 0)
                    {
                        int y = 15;
                        if ((data.type & 0x800000) == 0)
                        {
                            if ((data.type & 0x1000000) == 0)
                            {
                                for (num2 = 0; num2 < (data.level & 0xff); num2++)
                                {
                                    graphics.DrawImage(bStar[0], 149 - (12 * num2), 0x25, 11, 11);
                                }
                            }
                            else
                            {
                                for (num2 = 0; num2 < (data.level & 0xff); num2++)
                                {
                                    graphics.DrawImage(bStar[0], 149 - (12 * num2), 0x23, 11, 11);
                                }
                            y = 12;
                            }
                        }
                        else
                        {
                            for (num2 = 0; num2 < (data.level & 0xff); num2++)
                            {
                                graphics.DrawImage(bStar[1], 17 + (12 * num2), 0x23, 11, 11);
                            }
                            y = 12;
                        }
                        if (data.attribute == 1)
                        {
                            graphics.DrawImage(bAttributes[0], 0x90, y, 0x12, 0x12);
                        }
                        else if (data.attribute == 2)
                        {
                            graphics.DrawImage(bAttributes[1], 0x90, y, 0x12, 0x12);
                        }
                        else if (data.attribute == 4)
                        {
                            graphics.DrawImage(bAttributes[2], 0x90, y, 0x12, 0x12);
                        }
                        else if (data.attribute == 8)
                        {
                            graphics.DrawImage(bAttributes[3], 0x90, y, 0x12, 0x12);
                        }
                        else if (data.attribute == 0x10)
                        {
                            graphics.DrawImage(bAttributes[4], 0x90, y, 0x12, 0x12);
                        }
                        else if (data.attribute == 0x20)
                        {
                            graphics.DrawImage(bAttributes[5], 0x90, y, 0x12, 0x12);
                        }
                        else if (data.attribute == 0x40)
                        {
                            graphics.DrawImage(bAttributes[6], 0x90, y, 0x12, 0x12);
                        }
                        if ((data.type & 0x1000000) == 0)
                        {
                            if ((data.type & 0x800000) == 0)
                            {
                                if (data.attack >= 0)
                                {
                                    graphics.DrawString(data.attack.ToString(), numFont, Brushes.Black, (float)105f, (float)231f);
                                }
                                else
                                {
                                    graphics.DrawString("?", numFont, textBrush, (float)115f, (float)231f);
                                }
                                if (data.defence >= 0)
                                {
                                    graphics.DrawString(data.defence.ToString(), numFont, Brushes.Black, (float)142f, (float)231f);
                                }
                                else
                                {
                                    graphics.DrawString("?", numFont, textBrush, (float)152f, (float)231f);
                                }
                            }
                            else
                            {
                                if (data.attack >= 0)
                                {
                                    graphics.DrawString(data.attack.ToString(), numFont, Brushes.Black, (float)109f, (float)231f);
                                }
                                else
                                {
                                    graphics.DrawString("?", numFont, textBrush, (float)119f, (float)231f);
                                }
                                if (data.defence >= 0)
                                {
                                    graphics.DrawString(data.defence.ToString(), numFont, Brushes.Black, (float)145f, (float)231f);
                                }
                                else
                                {
                                    graphics.DrawString("?", numFont, textBrush, (float)155f, (float)231f);
                                }
                            }
                        }
                        else
                        {
                            if (data.attack >= 0)
                            {
                                graphics.DrawString(data.attack.ToString(), numFont, Brushes.Black, (float)105f, (float)231f);
                            }
                            else
                            {
                                graphics.DrawString("?", numFont, textBrush, (float)115f, (float)231f);
                            }
                            if (data.defence >= 0)
                            {
                                graphics.DrawString(data.defence.ToString(), numFont, Brushes.Black, (float)142f, (float)231f);
                            }
                            else
                            {
                                graphics.DrawString("?", numFont, textBrush, (float)152f, (float)231f);
                            }
                        }
                        if ((data.type & 0x800000) == 0 && (data.type & 0x1000000) == 0)
						{
                            graphics.DrawString(gettypestring(data), typeFont, typeBrush, (float) 13f, (float) 195f);
                            ef = graphics.MeasureString(text.text, txtFont, 0x91);
                            num4 = 0x91;
                            while (ef.Height > (((float) (0x19 * num4)) / 145f))
                            {
                                num4 += 3;
                                ef = graphics.MeasureString(text.text, txtFont, num4);
                            }
                            graphics.TranslateTransform(16f, 205f);
                            graphics.ScaleTransform(145f / ((float) num4), 145f / ((float) num4));
                            graphics.DrawString(text.text, txtFont, textBrush, new RectangleF(0f, 0f, ef.Width, ef.Height));
                            graphics.ResetTransform();
                        }
						else if ((data.type & 0x1000000) == 0)//xyz
						{
                            graphics.DrawString(gettypestring(data), typeFont, typeBrush, (float) 12f, (float) 192f);
                            ef = graphics.MeasureString(text.text, txtFont, 0x91);
                            num4 = 0x91;
                            while (ef.Height > (((float) (0x1c * num4)) / 145f))
                            {
                                num4 += 3;
                                ef = graphics.MeasureString(text.text, txtFont, num4);
                            }
                            graphics.TranslateTransform(16f, 202f);
                            graphics.ScaleTransform(145f / ((float) num4), 145f / ((float) num4));
                            graphics.DrawString(text.text, txtFont, textBrush, new RectangleF(0f, 0f, ef.Width, ef.Height));
                            graphics.ResetTransform();
						}
						else//pendulum
						{
							graphics.DrawString(gettypestring(data), typeFont, typeBrush, (float)12f, (float)192f);
							string monster_effect = GetDesc(text.text, regex_monster);
							string pendulum_effect = GetDesc(text.text, regex_pendulum);
                            int lscale = (data.level >> 0x18) & 0xff;
                            int rscale = (data.level >> 0x10) & 0xff;
                            if (lscale > 9)
                            {
                                graphics.DrawString(lscale.ToString(), new Font("文泉驿微米黑", 7f), Brushes.Black, (float)13f, (float)174f);
                            }
                            else
                            {
                                graphics.DrawString(lscale.ToString(), new Font("文泉驿微米黑", 8f), Brushes.Black, (float)16f, (float)174f);
                            }
                            if (rscale > 9)
                            {
                                graphics.DrawString(rscale.ToString(), new Font("文泉驿微米黑", 7f), Brushes.Black, (float)150f, (float)174f);
                            }
                            else
                            {
                                graphics.DrawString(rscale.ToString(), new Font("文泉驿微米黑", 8f), Brushes.Black, (float)151f, (float)174f);
                            }
                            ef = graphics.MeasureString(monster_effect, txtFont, 0x91);
							num4 = 0x91;
							while (ef.Height > (((float)(0x1c * num4)) / 145f))
							{
								num4 += 3;
								ef = graphics.MeasureString(monster_effect, txtFont, num4);
							}
							graphics.TranslateTransform(16f, 202f);
							graphics.ScaleTransform(145f / ((float)num4), 145f / ((float)num4));
							graphics.DrawString(monster_effect, txtFont, textBrush, new RectangleF(0f, 0f, ef.Width, ef.Height));
							graphics.ResetTransform();
                            int num5;
                            SizeF pf;
                            pf = graphics.MeasureString(pendulum_effect, txtFont, 0x77);
                            num5 = 0x77;
                            while (pf.Height > (((float)(0x18 * num5)) / 119f))
                            {
                                num5 += 1;
                                pf = graphics.MeasureString(pendulum_effect, txtFont, num5);
                            }
                            graphics.TranslateTransform(29f, 162f);
                            graphics.ScaleTransform(119f / ((float)num5), 119f / ((float)num5));
                            graphics.DrawString(pendulum_effect, txtFont, textBrush, new RectangleF(0f, 0f, pf.Width, pf.Height));
                            graphics.ResetTransform();
						}
					}
                    else
                    {
                        if ((data.type & 2) != 0)
                        {
                            if (data.type == 2)
                            {
                                graphics.DrawImage(bType[0], 0x65, 0x25);
                            }
                            else
                            {
                                graphics.DrawImage(bType[1], 0x5c, 0x25);
                                if ((data.type & 0x20000) != 0)
                                {
                                    graphics.DrawImage(bType[4], 0x8f, 40);
                                }
                                else if ((data.type & 0x40000) != 0)
                                {
                                    graphics.DrawImage(bType[6], 0x8f, 40);
                                }
                                else if ((data.type & 0x80000) != 0)
                                {
                                    graphics.DrawImage(bType[7], 0x8f, 40);
                                }
                                else if ((data.type & 0x10000) != 0)
                                {
                                    graphics.DrawImage(bType[8], 0x8f, 40);
                                }
                                else if ((data.type & 0x80) != 0)
                                {
                                    graphics.DrawImage(bType[9], 0x8f, 40);
                                }
                            }
                        }
                        else if ((data.type & 4) != 0)
                        {
                            if (data.type == 4)
                            {
                                graphics.DrawImage(bType[2], 0x6f, 0x25);
                            }
                            else
                            {
                                graphics.DrawImage(bType[3], 0x66, 0x25);
                                if ((data.type & 0x20000) != 0)
                                {
                                    graphics.DrawImage(bType[4], 0x8f, 40);
                                }
                                else if ((data.type & 0x100000) != 0)
                                {
                                    graphics.DrawImage(bType[5], 0x8f, 40);
                                }
                            }
                        }
                        ef = graphics.MeasureString(text.text, txtFont, 0x91);
                        num4 = 0x91;
                        while (ef.Height > (((float) (40 * num4)) / 145f))
                        {
                            num4 += 3;
                            ef = graphics.MeasureString(text.text, txtFont, num4);
                        }
                        graphics.TranslateTransform(16f, 195f);
                        graphics.ScaleTransform(145f / ((float) num4), 145f / ((float) num4));
                        graphics.DrawString(text.text, txtFont, textBrush, new RectangleF(0f, 0f, ef.Width, ef.Height));
                        graphics.ResetTransform();
                    }
                    try
                    {
                        Bitmap image = new Bitmap("./pico/" + code.ToString() + ".jpg");
                        if ((data.type & 0x1000000) > 0)
                        {
                            graphics.DrawImage(image, 0xF, 0x32, 0x93, 0x6d);
                        }
                        else if((data.type & 0x800000) == 0)
                        {
                            graphics.DrawImage(image, 0x19, 0x36, 0x80, 0x80);
                        }
                        else
                        {
                            graphics.DrawImage(image, 0x18, 0x33, 0x80, 0x80);
                        }
                    }
                    catch
                    {
                    }
                    string str3 = text.name.Replace('\x00b7', '・');
                    float width = graphics.MeasureString(str3, nameFont).Width;
                    float sx = 1f;
                    if (width > 125f)
                    {
                        sx *= 125f / width;
                    }
                    if ((data.type & 0x800000) > 0)
                    {
                        graphics.TranslateTransform(12f, 13f);
                    }
                    else if ((data.type & 0x1000000) > 0)
                    {
                        graphics.TranslateTransform(12f, 13f);
                    }
                    else
                    {
                        graphics.TranslateTransform(14f, 17f);
                    }
                    graphics.ScaleTransform(sx, 1f);
                    graphics.DrawString(str3, nameFont, nameBrush, (float) 0f, (float) 0f);
                    graphics.DrawString(str3, nameFont, Brushes.Gold, (float) 1f, (float) 1f);
                    graphics.ResetTransform();
                    cardImages.Add(code, bitmap_answer);
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
                    chArray[i] = (char) (chArray[i] + 0xfee0);
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

