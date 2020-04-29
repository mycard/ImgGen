namespace ImgGen
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.IO;
    using System.Threading;

    internal class Program
    {
        private static PrivateFontCollection fontCollection;
        private static ImageCodecInfo encoderInfo;
        private static EncoderParameters encoderParams;

        private static bool generateLarge;
        private static bool generateSmall;
        private static bool generateThumb;

        private static Queue<string> files;
        private static object locker = new object();

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

            encoderInfo = GetEncoderInfo("image/jpeg");
            encoderParams = new EncoderParameters(1);
            EncoderParameter parameter = new EncoderParameter(Encoder.Quality, 90L);
            encoderParams.Param[0] = parameter;

            if (args.Length > 0)
            {
                DataManager.InitialDatas(args[0]);
            }
            else
            {
                DataManager.InitialDatas("../cards.cdb");
            }

            files = new Queue<string>();
            foreach (string file in Directory.GetFiles("./pico", "*.png"))
            {
                files.Enqueue(file);
            }
            foreach (string file in Directory.GetFiles("./pico", "*.jpg"))
            {
                files.Enqueue(file);
            }

            generateLarge = System.Configuration.ConfigurationManager.AppSettings["GenerateLarge"] != "False"; // true if AppSettings null
            generateSmall = System.Configuration.ConfigurationManager.AppSettings["GenerateSmall"] == "True";
            generateThumb = System.Configuration.ConfigurationManager.AppSettings["GenerateThumb"] == "True";
            if (generateLarge)
                Directory.CreateDirectory("./picn");
            if (generateSmall)
                Directory.CreateDirectory("./pics");
            if (generateThumb)
                Directory.CreateDirectory("./pics/thumbnail");

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                Thread workThread = new Thread(Worker);
                workThread.Start();
            }
        }

        private static void Worker()
        {
            ImageManager imageManager;
            lock (locker)
            {
                imageManager = new ImageManager();
                imageManager.InitialDatas();
            }
            while (true)
            {
                string file;
                lock (locker)
                {
                    if (files.Count == 0)
                        return;
                    file = files.Dequeue();
                }
                Genernate(file, imageManager);
            }
        }

        private static void Genernate(string srcName, ImageManager imageManager)
        {
            int code;
            try
            {
                code = int.Parse(Path.GetFileNameWithoutExtension(srcName));
            }
            catch
            {
                return;
            }
            string fileName = code.ToString() + ".jpg";
            Console.WriteLine($"Generating {fileName}");
            Bitmap image = imageManager.GetImage(code);
            if (image == null)
            {
                Console.WriteLine($"[{code}] generation failed");
                return;
            }
            if (generateLarge)
            {
                image.Save("./picn/" + fileName, encoderInfo, encoderParams);
            }
            if (generateSmall)
            {
                Bitmap bmp = Zoom(image, 177, 254);
                bmp.Save("./pics/" + fileName, encoderInfo, encoderParams);
                bmp.Dispose();
            }
            if (generateThumb)
            {
                Bitmap bmp = Zoom(image, 44, 64);
                bmp.Save("./pics/thumbnail/" + fileName, encoderInfo, encoderParams);
                bmp.Dispose();
            }
            image?.Dispose();
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

