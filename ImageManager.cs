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

        private bool isLiShu = false;

        private Bitmap LoadBitmap(string bmp)
        {
            //Release file lock after bitmap loaded
            FileStream fs = new FileStream(bmp,FileMode.Open);
            Bitmap bitmap = new Bitmap(fs);
            Bitmap bitmap2 = (Bitmap)bitmap.Clone();
            bitmap.Dispose();
            fs.Dispose();
            return bitmap2;
        }

        public void InitialDatas()
        {
            string _xyzString = System.Configuration.ConfigurationManager.AppSettings["XyzString"];
            if (_xyzString != null)
            {
                this.xyzString = _xyzString;
            }

            string _zeroStarCards = System.Configuration.ConfigurationManager.AppSettings["ZeroStarCards"];
            if (_zeroStarCards != null)
            {
                foreach (string i in _zeroStarCards.Split(','))
                {
                    this.zeroStarCards.Add(int.Parse(i));
                }
            }

            this.numfontName = Program.GetFontFamily("MatrixBoldSmallCaps");
            string _style = System.Configuration.ConfigurationManager.AppSettings["Style"];
            if (_style == "隶书")
            {
                this.isLiShu = true;
                this.fontName = Program.GetFontFamily("方正隶变_GBK");
            }
            else
            {
                this.fontName = Program.GetFontFamily("文泉驿微米黑");
            }

            string _namestyle = System.Configuration.ConfigurationManager.AppSettings["NameStyle"];
            if (_namestyle == "金色")
            {
                this.nameBlackBrush = new SolidBrush(Color.FromArgb(255, 215, 0));
                this.nameWhiteBrush = new SolidBrush(Color.FromArgb(255, 215, 0));
                this.nameShadowBrush = new SolidBrush(Color.FromArgb(64, 64, 0));
                this.nameBlackFont = new Font(this.fontName, this.isLiShu ? 30 : 28, this.isLiShu ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);
                this.nameWhiteFont = new Font(this.fontName, this.isLiShu ? 30 : 28, this.isLiShu ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);
            }
            else
            {
                this.nameBlackBrush = new SolidBrush(Color.FromArgb(0, 0, 0));
                this.nameWhiteBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
                this.nameShadowBrush = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
                this.nameBlackFont = new Font(this.fontName, this.isLiShu ? 30 : 28, this.isLiShu ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);
                this.nameWhiteFont = new Font(this.fontName, this.isLiShu ? 30 : 28, FontStyle.Regular, GraphicsUnit.Pixel);
            }

            this.numFont = new Font(this.numfontName, 17, FontStyle.Bold, GraphicsUnit.Pixel);
            this.numUnknownFont = new Font(this.numfontName, 22, FontStyle.Bold, GraphicsUnit.Pixel);
            this.typeFont = new Font(this.fontName, 12, FontStyle.Bold, GraphicsUnit.Pixel);
            this.txtFont = new Font(this.fontName, 10, FontStyle.Bold, GraphicsUnit.Pixel);
            this.scaleFontNormal = new Font(this.numfontName, 30, GraphicsUnit.Pixel);
            this.scaleFontSmall = new Font(this.numfontName, 27, GraphicsUnit.Pixel);

            this.pendBgBrush = new SolidBrush(Color.FromArgb(0, 125, 105));
            this.textBrush = new SolidBrush(Color.FromArgb(0, 0, 0));

            this.justifyFormat = new StringFormat(StringFormat.GenericTypographic);
            this.justifyFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            this.justifyFormat.FormatFlags |= StringFormatFlags.FitBlackBox;
            this.rightAlignFormat = new StringFormat
            {
                Alignment = StringAlignment.Far
            };

            this.bTemplates[0] = this.LoadBitmap("./textures/card_spell.png");
            this.bTemplates[1] = this.LoadBitmap("./textures/card_trap.png");
            this.bTemplates[2] = this.LoadBitmap("./textures/card_synchro.png");
            this.bTemplates[3] = this.LoadBitmap("./textures/card_xyz.png");
            this.bTemplates[4] = this.LoadBitmap("./textures/card_fusion.png");
            this.bTemplates[5] = this.LoadBitmap("./textures/card_ritual.png");
            this.bTemplates[6] = this.LoadBitmap("./textures/card_token.png");
            this.bTemplates[7] = this.LoadBitmap("./textures/card_effect.png");
            this.bTemplates[8] = this.LoadBitmap("./textures/card_normal.png");
            this.bTemplates[9] = this.LoadBitmap("./textures/card_pxyz.png");
            this.bTemplates[10] = this.LoadBitmap("./textures/card_peffect.png");
            this.bTemplates[11] = this.LoadBitmap("./textures/card_pnormal.png");
            this.bTemplates[12] = this.LoadBitmap("./textures/card_psynchro.png");
            this.bTemplates[13] = this.LoadBitmap("./textures/card_pfusion.png");
            this.bTemplates[14] = this.LoadBitmap("./textures/card_link.png");
            this.bAttributes[0] = this.LoadBitmap("./textures/att_earth.png");
            this.bAttributes[1] = this.LoadBitmap("./textures/att_water.png");
            this.bAttributes[2] = this.LoadBitmap("./textures/att_fire.png");
            this.bAttributes[3] = this.LoadBitmap("./textures/att_wind.png");
            this.bAttributes[4] = this.LoadBitmap("./textures/att_light.png");
            this.bAttributes[5] = this.LoadBitmap("./textures/att_dark.png");
            this.bAttributes[6] = this.LoadBitmap("./textures/att_devine.png");
            this.bStar[0] = this.LoadBitmap("./textures/star.png");
            this.bStar[1] = this.LoadBitmap("./textures/starb.png");
            this.bType[0] = this.LoadBitmap("./textures/spell_normal.png");
            this.bType[1] = this.LoadBitmap("./textures/spell_quickplay.png");
            this.bType[2] = this.LoadBitmap("./textures/spell_continuous.png");
            this.bType[3] = this.LoadBitmap("./textures/spell_equip.png");
            this.bType[4] = this.LoadBitmap("./textures/spell_field.png");
            this.bType[5] = this.LoadBitmap("./textures/spell_ritual.png");
            this.bType[6] = this.LoadBitmap("./textures/trap_normal.png");
            this.bType[7] = this.LoadBitmap("./textures/trap_continuous.png");
            this.bType[8] = this.LoadBitmap("./textures/trap_counter.png");
            for (int i = 1; i <= 9; i++)
            {
                if (i < 9)
                {
                    this.bLinkNums[i - 1] = this.LoadBitmap($"./textures/link_{i}.png");
                }

                if (i == 5)
                {
                    continue;
                }
                this.bLinkMarkers[i - 1] = this.LoadBitmap($"./textures/link_marker_on_{i}.png");
            }
        }

        public Bitmap GetImage(int code)
        {
            Data data = DataManager.GetData(code);
            return this.DrawCard(data);
        }

        private void DrawPicture(Graphics graphics, Data data)
        {
            Bitmap image = null;
            string filename = "./pico/" + data.code.ToString() + ".jpg";
            //if (!File.Exists(filename))
            //    filename = "./pico/" + data.code.ToString() + ".png";
#if !DEBUG
            try
            {
#endif
            image = new Bitmap(filename); //Unique, no need to use this.LoadBitmap
#if !DEBUG
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when parsing {data.code} - {e}");
                return;
            }
#endif
            if (data.IsType(Type.TYPE_PENDULUM))
            {
                if (image.Width == 347 && image.Height == 444) // LOTDLE游戏图
                {
                    graphics.DrawImage(image, 27, 103, 347, 444);
                }
                else
                {
                    graphics.FillRectangle(this.pendBgBrush, new Rectangle(23, 362, 354, 189));
                    Rectangle dest = new Rectangle(27, 103, 347, 259);
                    if (image.Width == 407 && image.Height == 593) // 官网图
                    {
                        graphics.DrawImage(image, dest, new Rectangle(27, 106, 353, 263), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        graphics.DrawImage(image, dest);
                    }
                }
            }
            else
            {
                Rectangle dest = new Rectangle(48, 106, 304, 304);
                if (image.Width == 407 && image.Height == 593) // 官网图
                {
                    graphics.DrawImage(image, dest, new Rectangle(49, 109, 308, 308), GraphicsUnit.Pixel);
                }
                else if (image.Width == 1030 && image.Height == 740) // 官推图
                {
                    graphics.DrawImage(image, dest, new Rectangle(100, 164, 339, 339), GraphicsUnit.Pixel);
                }
                else
                {
                    graphics.DrawImage(image, dest);
                }
            }
            image?.Dispose();
        }

        private void DrawTemplate(Graphics graphics, Data data)
        {
            Bitmap template;
            if (data.IsType(Type.TYPE_SPELL))
            {
                template = this.bTemplates[0];
            }
            else if (data.IsType(Type.TYPE_TRAP))
            {
                template = this.bTemplates[1];
            }
            else if (data.IsType(Type.TYPE_PENDULUM))
            {
                if (data.IsType(Type.TYPE_SYNCHRO))
                {
                    template = this.bTemplates[12];
                }
                else if (data.IsType(Type.TYPE_XYZ))
                {
                    template = this.bTemplates[9];
                }
                else if (data.IsType(Type.TYPE_FUSION))
                {
                    template = this.bTemplates[13];
                }
                else if (data.IsType(Type.TYPE_EFFECT))
                {
                    template = this.bTemplates[10];
                }
                else //pnormal
                {
                    template = this.bTemplates[11];
                }
            }
            else if (data.IsType(Type.TYPE_LINK))
            {
                template = this.bTemplates[14];
            }
            else if (data.IsType(Type.TYPE_SYNCHRO))
            {
                template = this.bTemplates[2];
            }
            else if (data.IsType(Type.TYPE_XYZ))
            {
                template = this.bTemplates[3];
            }
            else if (data.IsType(Type.TYPE_FUSION))
            {
                template = this.bTemplates[4];
            }
            else if (data.IsType(Type.TYPE_RITUAL))
            {
                template = this.bTemplates[5];
            }
            else if (data.IsType(Type.TYPE_TOKEN))
            {
                template = this.bTemplates[6];
            }
            else if (data.IsType(Type.TYPE_EFFECT))
            {
                template = this.bTemplates[7];
            }
            else //normal
            {
                template = this.bTemplates[8];
            }
            graphics.DrawImage(template, 0, 0, 400, 580);
        }

        private void DrawStars(Graphics graphics, Data data)
        {
            if (!this.zeroStarCards.Contains(data.code))
            {
                int nStar;
                int level = data.level & 0xff;
                if (data.IsType(Type.TYPE_XYZ))
                {
                    for (nStar = 0; nStar < level; nStar++)
                    {
                        graphics.DrawImage(this.bStar[1], 41f + (26.5f * nStar), 69, 28, 28);
                    }
                }
                else if (!data.IsType(Type.TYPE_LINK))
                {
                    for (nStar = 0; nStar < level; nStar++)
                    {
                        graphics.DrawImage(this.bStar[0], 332f - (26.5f * nStar), 69, 28, 28);
                    }
                }
            }
        }

        private void DrawAttributes(Graphics graphics, Data data)
        {
            int nAttr = -1;
            if (data.attribute == Attribute.ATTRIBUTE_EARTH)
            {
                nAttr = 0;
            }
            else if (data.attribute == Attribute.ATTRIBUTE_WATER)
            {
                nAttr = 1;
            }
            else if (data.attribute == Attribute.ATTRIBUTE_FIRE)
            {
                nAttr = 2;
            }
            else if (data.attribute == Attribute.ATTRIBUTE_WIND)
            {
                nAttr = 3;
            }
            else if (data.attribute == Attribute.ATTRIBUTE_LIGHT)
            {
                nAttr = 4;
            }
            else if (data.attribute == Attribute.ATTRIBUTE_DARK)
            {
                nAttr = 5;
            }
            else if (data.attribute == Attribute.ATTRIBUTE_DEVINE)
            {
                nAttr = 6;
            }

            if (nAttr >= 0)
            {
                graphics.DrawImage(this.bAttributes[nAttr], 334, 28, 36, 36);
            }
        }

        private void DrawAtkDef(Graphics graphics, Data data)
        {
            if (data.attack >= 0)
            {
                graphics.DrawString(data.attack.ToString(), this.numFont, this.textBrush, new Rectangle(248, 530, 42, 17), this.rightAlignFormat);
            }
            else
            {
                graphics.DrawString("?", this.numUnknownFont, this.textBrush, 274, 527);
            }

            if (data.IsType(Type.TYPE_LINK))
            {
                graphics.DrawImage(this.bLinkNums[data.level - 1], 353, 530, 13, 13);
            }
            else
            {
                if (data.defence >= 0)
                {
                    graphics.DrawString(data.defence.ToString(), this.numFont, this.textBrush, new Rectangle(329, 530, 42, 17), this.rightAlignFormat);
                }
                else
                {
                    graphics.DrawString("?", this.numUnknownFont, this.textBrush, 355, 527);
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
                case Race.RACE_REPTILE: str += "爬虫族"; break;
                case Race.RACE_PSYCHO: str += "念动力族"; break;
                case Race.RACE_DEVINE: str += "幻神兽族"; break;
                case Race.RACE_CREATORGOD: str += "创造神族"; break;
                case Race.RACE_WYRM: str += "幻龙族"; break;
                case Race.RACE_CYBERS: str += "电子界族"; break;
                default: str += "???"; break;
            }
            if (data.IsType(Type.TYPE_FUSION))
            {
                str += "／融合";
            }

            if (data.IsType(Type.TYPE_SYNCHRO))
            {
                str += "／同调";
            }

            if (data.IsType(Type.TYPE_LINK))
            {
                str += "／连接";
            }

            if (data.IsType(Type.TYPE_XYZ))
            {
                str = str + "／" + this.xyzString;
            }

            if (data.IsType(Type.TYPE_RITUAL))
            {
                str += "／仪式";
            }

            if (data.IsType(Type.TYPE_SPSUMMON))
            {
                str += "／特殊召唤";
            }

            if (data.IsType(Type.TYPE_PENDULUM))
            {
                str += "／灵摆";
            }

            if (data.IsType(Type.TYPE_SPIRIT))
            {
                str += "／灵魂";
            }

            if (data.IsType(Type.TYPE_DUAL))
            {
                str += "／二重";
            }

            if (data.IsType(Type.TYPE_UNION))
            {
                str += "／同盟";
            }

            if (data.IsType(Type.TYPE_FLIP))
            {
                str += "／反转";
            }

            if (data.IsType(Type.TYPE_TOON))
            {
                str += "／卡通";
            }

            if (data.IsType(Type.TYPE_TUNER))
            {
                str += "／调整";
            }

            if (data.IsType(Type.TYPE_EFFECT))
            {
                str += "／效果";
            }

            if (data.IsType(Type.TYPE_NORMAL))
            {
                str += "／通常";
            }

            str += "】";

            float tWidth = graphics.MeasureString(str, this.typeFont).Width;
            float sx1 = 1f;
            if (tWidth > 330f)
            {
                sx1 *= 330f / tWidth;
            }
            graphics.ScaleTransform(sx1, 1f);
            graphics.DrawString(str, this.typeFont, this.textBrush, 26, 438);
            if (this.isLiShu)
            {
                graphics.DrawString(str, this.typeFont, this.textBrush, 26, 438);
            }

            graphics.ResetTransform();
        }

        private void DrawScales(Graphics graphics, Data data)
        {
            int lscale = (data.level >> 0x18) & 0xff;
            int rscale = (data.level >> 0x10) & 0xff;
            if (lscale > 9)
            {
                graphics.DrawString("1", this.scaleFontSmall, this.textBrush, 26, 397);
                graphics.DrawString((lscale - 10).ToString(), this.scaleFontSmall, this.textBrush, 37, 397);
            }
            else
            {
                graphics.DrawString(lscale.ToString(), this.scaleFontNormal, this.textBrush, 31, 396);
            }
            if (rscale > 9)
            {
                graphics.DrawString("1", this.scaleFontSmall, this.textBrush, 341, 397);
                graphics.DrawString((rscale - 10).ToString(), this.scaleFontSmall, this.textBrush, 352, 397);
            }
            else
            {
                graphics.DrawString(rscale.ToString(), this.scaleFontNormal, this.textBrush, 346, 396);
            }
        }

        private void DrawMonsterEffect(Graphics graphics, string text)
        {
            this.DrawJustifiedText(graphics, text, 33, 453, 335, 75);
            if (this.isLiShu)
            {
                this.DrawJustifiedText(graphics, text, 33, 453, 335, 75);
            }
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
            {
                pText = match.Groups[match.Groups.Count - 1].Value;
            }

            regex = new Regex(regex_monster, RegexOptions.Multiline);
            match = regex.Match(desc);
            if (match.Success)
            {
                mText = match.Groups[match.Groups.Count - 1].Value;
            }

            this.DrawJustifiedText(graphics, pText, 65, 369, 272, 58);
            if (this.isLiShu)
            {
                this.DrawJustifiedText(graphics, pText, 65, 369, 272, 58);
            }

            this.DrawMonsterEffect(graphics, mText);
        }

        private void DrawSpellTrapEffect(Graphics graphics, string text)
        {
            this.DrawJustifiedText(graphics, text, 33, 439, 335, 108);
            if (this.isLiShu)
            {
                this.DrawJustifiedText(graphics, text, 33, 439, 335, 108);
            }
        }

        private void DrawSpellTrapType(Graphics graphics, Data data)
        {
            if (data.IsType(Type.TYPE_SPELL))
            {
                int nType = 0;
                if (data.IsType(Type.TYPE_QUICKPLAY))
                {
                    nType = 1;
                }

                if (data.IsType(Type.TYPE_CONTINUOUS))
                {
                    nType = 2;
                }

                if (data.IsType(Type.TYPE_EQUIP))
                {
                    nType = 3;
                }

                if (data.IsType(Type.TYPE_FIELD))
                {
                    nType = 4;
                }

                if (data.IsType(Type.TYPE_RITUAL))
                {
                    nType = 5;
                }

                graphics.DrawImage(this.bType[nType], 221, 69, 137, 26);
            }
            else if (data.IsType(Type.TYPE_TRAP))
            {
                int nType = 6;
                if (data.IsType(Type.TYPE_CONTINUOUS))
                {
                    nType = 7;
                }

                if (data.IsType(Type.TYPE_COUNTER))
                {
                    nType = 8;
                }

                graphics.DrawImage(this.bType[nType], 243, 68, 115, 27);
            }
        }

        private void DrawLinkMarkers(Graphics graphics, Data data)
        {
            LinkMarker lm = (LinkMarker)data.defence;
            if ((lm & LinkMarker.LINK_MARKER_BOTTOM_LEFT) > 0)
            {
                graphics.DrawImage(this.bLinkMarkers[0], 34, 387, 38, 37);
            }

            if ((lm & LinkMarker.LINK_MARKER_BOTTOM) > 0)
            {
                graphics.DrawImage(this.bLinkMarkers[1], 163, 406, 73, 25);
            }

            if ((lm & LinkMarker.LINK_MARKER_BOTTOM_RIGHT) > 0)
            {
                graphics.DrawImage(this.bLinkMarkers[2], 329, 387, 37, 37);
            }

            if ((lm & LinkMarker.LINK_MARKER_LEFT) > 0)
            {
                graphics.DrawImage(this.bLinkMarkers[3], 27, 222, 24, 72);
            }

            if ((lm & LinkMarker.LINK_MARKER_RIGHT) > 0)
            {
                graphics.DrawImage(this.bLinkMarkers[5], 349, 221, 24, 72);
            }

            if ((lm & LinkMarker.LINK_MARKER_TOP_LEFT) > 0)
            {
                graphics.DrawImage(this.bLinkMarkers[6], 34, 91, 37, 37);
            }

            if ((lm & LinkMarker.LINK_MARKER_TOP) > 0)
            {
                graphics.DrawImage(this.bLinkMarkers[7], 163, 85, 74, 23);
            }

            if ((lm & LinkMarker.LINK_MARKER_TOP_RIGHT) > 0)
            {
                graphics.DrawImage(this.bLinkMarkers[8], 329, 91, 37, 37);
            }
        }

        private void DrawName(Graphics graphics, string name, Data data)
        {
            Font nameFont = this.nameBlackFont;
            Brush nameBrush = this.nameBlackBrush;
            if (data.IsType(Type.TYPE_SPELL | Type.TYPE_TRAP | Type.TYPE_FUSION | Type.TYPE_XYZ | Type.TYPE_LINK))
            {
                nameFont = this.nameWhiteFont;
                nameBrush = this.nameWhiteBrush;
            }
            SizeF size = graphics.MeasureString(name, nameFont);
            float width = size.Width;
            float sx = 1f;
            if (width > 308f)
            {
                sx *= 308f / width;
            }
            graphics.TranslateTransform(27, this.isLiShu ? 29.5f : 28.5f);
            graphics.ScaleTransform(sx, 1f);
            graphics.DrawString(name, nameFont, this.nameShadowBrush, 0f, 0f);
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

            this.DrawPicture(graphics, data);
            this.DrawTemplate(graphics, data);

            this.DrawName(graphics, name, data);

            if (data.IsType(Type.TYPE_MONSTER))
            {
                this.DrawStars(graphics, data);
                this.DrawAttributes(graphics, data);
                this.DrawMonsterType(graphics, data);
                this.DrawAtkDef(graphics, data);

                if (data.IsType(Type.TYPE_PENDULUM))
                {
                    this.DrawScales(graphics, data);
                    this.DrawPendulumEffect(graphics, desc);
                }
                else
                {
                    this.DrawMonsterEffect(graphics, desc);
                }

                if (data.IsType(Type.TYPE_LINK))
                {
                    this.DrawLinkMarkers(graphics, data);
                }
            }
            else
            {
                this.DrawSpellTrapType(graphics, data);
                this.DrawSpellTrapEffect(graphics, desc);
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

            float size = this.txtFont.Size;
            var font = new Font(this.txtFont.Name, size, this.txtFont.Style, this.txtFont.Unit);
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
                    string nextword = this.GetNextWord(text, pos);
                    if (word == "\n")
                    {
                        lines.Add(line);
                        paddings.Add(0);
                        line = "";
                        linewidth = 0;
                        pos++;
                        continue;
                    }
                    SizeF doublesize = graphics.MeasureString(word + nextword, font, 99, this.justifyFormat);
                    SizeF singlesize = graphics.MeasureString(word, font, 99, this.justifyFormat);
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
                if (lines.Count * (size + this.GetLineSpacing(size)) <= areaHeight - this.GetLineSpacing(size))
                {
                    break;
                }

                size -= 0.5f;
                font = new Font(this.txtFont.Name, size, this.txtFont.Style, this.txtFont.Unit);
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
                    string nextword = this.GetNextWord(line, pos);

                    if (word == "●" && !this.isLiShu)
                    {
                        Font spFont = new Font(this.spfontName, size * 0.9f, this.txtFont.Style, this.txtFont.Unit);
                        graphics.DrawString(word, spFont, this.textBrush, dx + (size / 10f), dy + (size / 10f), this.justifyFormat);
                    }
                    else if (word == "×" && !this.isLiShu)
                    {
                        Font spFont = new Font(this.spfontName, size, this.txtFont.Style, this.txtFont.Unit);
                        graphics.DrawString(word, spFont, this.textBrush, dx + (size / 3f), dy + (size / 20f), this.justifyFormat);
                    }
                    else if (word == "量" && !this.isLiShu)
                    {
                        Font spFont = new Font(this.spfontName, size, this.txtFont.Style, this.txtFont.Unit);
                        graphics.DrawString(word, spFont, this.textBrush, dx, dy + (size / 20f), this.justifyFormat);
                    }
                    else if (word[0] >= '\xff10' && word[0] <= '\xff19' && this.isLiShu) // 0-9数字
                    {
                        graphics.DrawString(word, font, this.textBrush, dx, dy - (size / 8f), this.justifyFormat);
                    }
                    else if (word[0] >= '\x2460' && word[0] <= '\x2469' && this.isLiShu) // ①-⑩数字
                    {
                        Font spFont = new Font(this.fontName, size * 0.8f, this.txtFont.Style, this.txtFont.Unit);
                        graphics.DrawString(word, spFont, this.textBrush, dx + (size / 10f), dy + (size / 10f), this.justifyFormat);
                    }
                    else
                    {
                        graphics.DrawString(word, font, this.textBrush, dx, dy, this.justifyFormat);
                    }

                    SizeF doublesize = graphics.MeasureString(word + nextword, font, 99, this.justifyFormat);
                    SizeF singlesize = graphics.MeasureString(word, font, 99, this.justifyFormat);
                    float dw = doublesize.Width - singlesize.Width;
                    dx += dw + exspace;
                }
                dx = areaX;
                dy += size + this.GetLineSpacing(size);
            }
        }
    }
}
