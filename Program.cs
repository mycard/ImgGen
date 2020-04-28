namespace ImgGen
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.IO;

    internal class Program
    {
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
            for (int i = 0; i < imageEncoders.Length; i++)
            {
                if (imageEncoders[i].MimeType == mimeType)
                {
                    return imageEncoders[i];
                }
            }
            return null;
        }

        private static PrivateFontCollection fontCollection;

        public static FontFamily GetFontFamily(string fontName)
        {
            try
            {
                return new FontFamily(fontName, fontCollection);
            }
            catch
            {
                try
                {
                    return new FontFamily(fontName);
                }
                catch
                {
                    Console.WriteLine($"Font {fontName} not found!");
                    return new FontFamily("Arial");
                }
            }
        }

        private static void Main(string[] args)
        {
            fontCollection = new PrivateFontCollection();
            foreach (string font in Directory.GetFiles("./fonts"))
            {
                fontCollection.AddFontFile(font);
            }
            if (args.Length > 0)
            {
                DataManager.InitialDatas(args[0]);
            }
            else
            {
                DataManager.InitialDatas("../cards.cdb");
            }
            Encoder quality = Encoder.Quality;
            ImageCodecInfo encoderInfo = GetEncoderInfo("image/jpeg");
            EncoderParameters encoderParams = new EncoderParameters(1);
            EncoderParameter parameter = new EncoderParameter(quality, 90L);
            encoderParams.Param[0] = parameter;
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles("./pico", "*.png"));
            files.AddRange(Directory.GetFiles("./pico", "*.jpg"));
            bool generateLarge = System.Configuration.ConfigurationManager.AppSettings["GenerateLarge"] != "False"; // true if AppSettings null
            bool generateSmall = System.Configuration.ConfigurationManager.AppSettings["GenerateSmall"] == "True";
            bool generateThumb = System.Configuration.ConfigurationManager.AppSettings["GenerateThumb"] == "True";
            if (generateLarge)
                Directory.CreateDirectory("./picn");
            if (generateSmall)
                Directory.CreateDirectory("./pics");
            if (generateThumb)
                Directory.CreateDirectory("./pics/thumbnail");
            foreach (string str in files)
            {
                int code;
                try
                {
                    code = int.Parse(Path.GetFileNameWithoutExtension(str));
                }
                catch
                {
                    continue;
                }
                string fileName = code.ToString() + ".jpg";
                Console.WriteLine($"Generating {fileName}");
                Bitmap image = DataManager.GetImage(code);
                if (image == null)
                {
                    Console.WriteLine($"[{code}] generation failed");
                    continue;
                }
                if (generateLarge)
                {
                    image.Save("./picn/" + fileName, encoderInfo, encoderParams);
                }
                if (generateSmall)
                {
                    Bitmap bmp = DataManager.Zoom(image, 177, 254);
                    bmp.Save("./pics/" + fileName, encoderInfo, encoderParams);
                    bmp.Dispose();
                }
                if (generateThumb)
                {
                    Bitmap bmp = DataManager.Zoom(image, 44, 64);
                    bmp.Save("./pics/thumbnail/" + fileName, encoderInfo, encoderParams);
                    bmp.Dispose();
                }
                image?.Dispose();
            }
        }
    }
}

