using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace ImgGen
{
    class ImageManager
    {
        private Bitmap[] bAttributes = new Bitmap[8];
        private Bitmap[] bStar = new Bitmap[2];
        private Bitmap[] bTemplates = new Bitmap[15];
        private Bitmap[] bType = new Bitmap[9];
        private Bitmap[] bLinkNums = new Bitmap[8];
        private Bitmap[] bLinkMarkers = new Bitmap[9];
        private Font nameBlackFont;
        private Font nameWhiteFont;
        private Font numFont;
        private Font numUnknownFont;
        private Font txtFont;
        private Font typeFont;
        private Font scaleFontNormal;
        private Font scaleFontSmall;
        private SolidBrush nameBlackBrush;
        private SolidBrush nameWhiteBrush;
        private SolidBrush nameShadowBrush;
        private SolidBrush pendBgBrush;
        private SolidBrush textBrush;

        private StringFormat justifyFormat;
        private StringFormat rightAlignFormat;

        private string xyzString = "超量";
        private string spfontName = "黑体";
        private FontFamily fontName;
        private FontFamily numfontName;

        private List<int> zeroStarCards = new List<int>();

        private bool LiShu = false;

        public void InitialDatas()
        {
            string _xyzString = System.Configuration.ConfigurationManager.AppSettings["XyzString"];
            if (_xyzString != null)
                xyzString = _xyzString;

            string _zeroStarCards = System.Configuration.ConfigurationManager.AppSettings["ZeroStarCards"];
            if (_zeroStarCards != null)
            {
                foreach (string i in _zeroStarCards.Split(','))
                {
                    zeroStarCards.Add(int.Parse(i));
                }
            }

            numfontName = Program.GetFontFamily("MatrixBoldSmallCaps");
            string _style = System.Configuration.ConfigurationManager.AppSettings["Style"];
            if (_style == "隶书")
            {
                LiShu = true;
                fontName = Program.GetFontFamily("方正隶变_GBK");
            }
            else
            {
                fontName = Program.GetFontFamily("文泉驿微米黑");
            }

            string _namestyle = System.Configuration.ConfigurationManager.AppSettings["NameStyle"];
            if (_namestyle == "金色")
            {
                nameBlackBrush = new SolidBrush(Color.FromArgb(255, 215, 0));
                nameWhiteBrush = new SolidBrush(Color.FromArgb(255, 215, 0));
                nameShadowBrush = new SolidBrush(Color.FromArgb(64, 64, 0));
                nameBlackFont = new Font(fontName, LiShu ? 30 : 28, LiShu ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);
                nameWhiteFont = new Font(fontName, LiShu ? 30 : 28, LiShu ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);
            }
            else
            {
                nameBlackBrush = new SolidBrush(Color.FromArgb(0, 0, 0));
                nameWhiteBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
                nameShadowBrush = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
                nameBlackFont = new Font(fontName, LiShu ? 30 : 28, LiShu ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);
                nameWhiteFont = new Font(fontName, LiShu ? 30 : 28, FontStyle.Regular, GraphicsUnit.Pixel);
            }

            numFont = new Font(numfontName, 17, FontStyle.Bold, GraphicsUnit.Pixel);
            numUnknownFont = new Font(numfontName, 22, FontStyle.Bold, GraphicsUnit.Pixel);
            typeFont = new Font(fontName, 12, FontStyle.Bold, GraphicsUnit.Pixel);
            txtFont = new Font(fontName, 10, FontStyle.Bold, GraphicsUnit.Pixel);
            scaleFontNormal = new Font(numfontName, 30, GraphicsUnit.Pixel);
            scaleFontSmall = new Font(numfontName, 27, GraphicsUnit.Pixel);

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
                    bLinkNums[i - 1] = new Bitmap($"./textures/link_{i}.png");
                if (i == 5) continue;
                bLinkMarkers[i - 1] = new Bitmap($"./textures/link_marker_on_{i}.png");
            }
        }

        public Bitmap GetImage(int code)
        {
            Data data = DataManager.GetData(code);
            return DrawCard(data);
        }

        private void DrawPicture(Graphics graphics, Data data)
        {
            Bitmap image = null;
            string filename = "./pico/" + data.code.ToString() + ".jpg";
            if (!File.Exists(filename))
                filename = "./pico/" + data.code.ToString() + ".png";
            try
            {
                image = new Bitmap(filename);
            }
            catch (Exception e)
#if DEBUG
                when (false)
#endif
            {
                Console.WriteLine($"Error when parsing {data.code} - {e}");
                return;
            }
            if (data.isType(Type.TYPE_PENDULUM))
            {
                if (image.Width == 347 && image.Height == 444) // LOTDLE游戏图
                    graphics.DrawImage(image, 27, 103, 347, 444);
                else
                {
                    graphics.FillRectangle(pendBgBrush, new Rectangle(23, 362, 354, 189));
                    Rectangle dest = new Rectangle(27, 103, 347, 259);
                    if (image.Width == 407 && image.Height >= 593) // 官网图
                        graphics.DrawImage(image, dest, new Rectangle(27, 106, 353, 263), GraphicsUnit.Pixel);
                    else
                        graphics.DrawImage(image, dest);
                }
            }
            else
            {
                Rectangle dest = new Rectangle(48, 106, 304, 304);
                if (image.Width == 407 && image.Height >= 593) // 官网图
                    graphics.DrawImage(image, dest, new Rectangle(49, 109, 308, 308), GraphicsUnit.Pixel);
                else if (image.Width == 1030 && image.Height == 740) // 官推图
                    graphics.DrawImage(image, dest, new Rectangle(100, 164, 339, 339), GraphicsUnit.Pixel);
                else
                    graphics.DrawImage(image, dest);
            }
            image?.Dispose();
        }

        private void DrawTemplate(Graphics graphics, Data data)
        {
            Bitmap template;
            if (data.isType(Type.TYPE_SPELL))
            {
                template = bTemplates[0];
            }
            else if (data.isType(Type.TYPE_TRAP))
            {
                template = bTemplates[1];
            }
            else if (data.isType(Type.TYPE_PENDULUM))
            {
                if (data.isType(Type.TYPE_SYNCHRO))
                {
                    template = bTemplates[12];
                }
                else if (data.isType(Type.TYPE_XYZ))
                {
                    template = bTemplates[9];
                }
                else if (data.isType(Type.TYPE_FUSION))
                {
                    template = bTemplates[13];
                }
                else if (data.isType(Type.TYPE_EFFECT))
                {
                    template = bTemplates[10];
                }
                else //pnormal
                {
                    template = bTemplates[11];
                }
            }
            else if (data.isType(Type.TYPE_LINK))
            {
                template = bTemplates[14];
            }
            else if (data.isType(Type.TYPE_SYNCHRO))
            {
                template = bTemplates[2];
            }
            else if (data.isType(Type.TYPE_XYZ))
            {
                template = bTemplates[3];
            }
            else if (data.isType(Type.TYPE_FUSION))
            {
                template = bTemplates[4];
            }
            else if (data.isType(Type.TYPE_RITUAL))
            {
                template = bTemplates[5];
            }
            else if (data.isType(Type.TYPE_TOKEN))
            {
                template = bTemplates[6];
            }
            else if (data.isType(Type.TYPE_EFFECT))
            {
                template = bTemplates[7];
            }
            else //normal
            {
                template = bTemplates[8];
            }
            graphics.DrawImage(template, 0, 0, 400, 580);
        }

        private void DrawStars(Graphics graphics, Data data)
        {
            if (!zeroStarCards.Contains(data.code))
            {
                int nStar;
                int level = data.level & 0xff;
                if (data.isType(Type.TYPE_XYZ))
                {
                    for (nStar = 0; nStar < level; nStar++)
                    {
                        graphics.DrawImage(bStar[1], (level==13  ? 27.5f : 41f) + (26.5f * nStar), 69, 28, 28);
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

        private void DrawAttributes(Graphics graphics, Data data)
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

        private void DrawAtkDef(Graphics graphics, Data data)
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

        private void DrawMonsterType(Graphics graphics, Data data)
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
                case Race.RACE_REPTILE: str += "爬虫类族"; break;
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
            if (LiShu)
                graphics.DrawString(str, typeFont, textBrush, 26, 438);
            graphics.ResetTransform();
        }

        private void DrawScales(Graphics graphics, Data data)
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

        private void DrawMonsterEffect(Graphics graphics, string text)
        {
            DrawJustifiedText(graphics, text, 33, 453, 335, 75);
            if (LiShu)
                DrawJustifiedText(graphics, text, 33, 453, 335, 75);
        }

        private void DrawPendulumEffect(Graphics graphics, string desc)
        {
            const string regex_pendulum = @"】[\s\S]*?\n([\S\s]*?)\n【";
            const string regex_monster = @"[果|介|述|報]】\n([\S\s]*)";

            string pText = "";
            string mText = "";

            Regex regex = new Regex(regex_pendulum, RegexOptions.Multiline);
            Match match = regex.Match(desc);
            if (match.Success)
                pText = match.Groups[match.Groups.Count - 1].Value;

            regex = new Regex(regex_monster, RegexOptions.Multiline);
            match = regex.Match(desc);
            if (match.Success)
                mText = match.Groups[match.Groups.Count - 1].Value;
            DrawJustifiedText(graphics, pText, 65, 369, 272, 58);
            if (LiShu)
                DrawJustifiedText(graphics, pText, 65, 369, 272, 58);
            DrawMonsterEffect(graphics, mText);
        }

        private void DrawSpellTrapEffect(Graphics graphics, string text)
        {
            DrawJustifiedText(graphics, text, 33, 439, 335, 108);
            if (LiShu)
                DrawJustifiedText(graphics, text, 33, 439, 335, 108);
        }

        private void DrawSpellTrapType(Graphics graphics, Data data)
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

        private void DrawLinkMarkers(Graphics graphics, Data data)
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

        private void DrawName(Graphics graphics, string name, Data data)
        {
            Font nameFont = nameBlackFont;
            Brush nameBrush = nameBlackBrush;
            if (data.isType(Type.TYPE_SPELL | Type.TYPE_TRAP | Type.TYPE_FUSION | Type.TYPE_XYZ | Type.TYPE_LINK))
            {
                nameFont = nameWhiteFont;
                nameBrush = nameWhiteBrush;
            }
            SizeF size = graphics.MeasureString(name, nameFont);
            float width = size.Width;
            float sx = 1f;
            if (width > 308f)
            {
                sx *= 308f / width;
            }
            graphics.TranslateTransform(27, LiShu ? 29.5f : 28.5f);
            graphics.ScaleTransform(sx, 1f);
            graphics.DrawString(name, nameFont, nameShadowBrush, 0f, 0f);
            graphics.DrawString(name, nameFont, nameBrush, 1f, 1f);
            graphics.ResetTransform();
        }

        private Bitmap DrawCard(Data data)
        {
            Bitmap bitmap = new Bitmap(400, 580);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            string name = DataManager.FormatCardName(data.name);
            string desc = DataManager.FormatCardDesc(data.text);

            DrawPicture(graphics, data);
            DrawTemplate(graphics, data);

            DrawName(graphics, name, data);

            if (data.isType(Type.TYPE_MONSTER))
            {
                DrawStars(graphics, data);
                DrawAttributes(graphics, data);
                DrawMonsterType(graphics, data);
                DrawAtkDef(graphics, data);

                if (data.isType(Type.TYPE_PENDULUM))
                {
                    DrawScales(graphics, data);
                    DrawPendulumEffect(graphics, desc);
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
            graphics.Dispose();
            return bitmap;
        }

        private string GetNextWord(string text, int pos)
        {
            return pos < text.Length - 1 ? text.Substring(pos + 1, 1) : "咕";
        }

        private float GetLineSpacing(float size)
        {
            return size / 5f;
        }

        private void DrawJustifiedText(Graphics graphics, string text, float areaX, float areaY, float areaWidth, float areaHeight)
        {
            const string non_start_chars = @"。；：，、”」）·× ";
            const string non_end_chars = @"“「（●";

            float size = txtFont.Size;
            var font = new Font(fontName, size, txtFont.Style, txtFont.Unit);
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
                font = new Font(fontName, size, txtFont.Style, txtFont.Unit);
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

                    if (word == "●" && !LiShu)
                    {
                        Font spFont = new Font(spfontName, size * 0.9f, txtFont.Style, txtFont.Unit);
                        graphics.DrawString(word, spFont, textBrush, dx + (size / 10f), dy + (size / 10f), justifyFormat);
                    }
                    else if (word == "×" && !LiShu)
                    {
                        Font spFont = new Font(spfontName, size, txtFont.Style, txtFont.Unit);
                        graphics.DrawString(word, spFont, textBrush, dx + (size / 3f), dy + (size / 20f), justifyFormat);
                    }
                    else if (word == "量" && !LiShu)
                    {
                        Font spFont = new Font(spfontName, size, txtFont.Style, txtFont.Unit);
                        graphics.DrawString(word, spFont, textBrush, dx, dy + (size / 20f), justifyFormat);
                    }
                    else if (word[0] >= '\xff10' && word[0] <= '\xff19' && LiShu) // 0-9数字
                    {
                        graphics.DrawString(word, font, textBrush, dx, dy - (size / 8f), justifyFormat);
                    }
                    else if (word[0] >= '\x2460' && word[0] <= '\x2469' && LiShu) // ①-⑩数字
                    {
                        Font spFont = new Font(fontName, size * 0.8f, txtFont.Style, txtFont.Unit);
                        graphics.DrawString(word, spFont, textBrush, dx + (size / 10f), dy + (size / 10f), justifyFormat);
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
    }
}
