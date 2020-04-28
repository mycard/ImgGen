namespace ImgGen
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
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

        private static void Main(string[] args)
        {
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
            bool generateSmall = System.Configuration.ConfigurationManager.AppSettings["GenerateSmall"] == "True";
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
                catch (Exception)
                {
                    continue;
                }
                string fileName = code.ToString() + ".jpg";
                Console.WriteLine($"Generating {fileName}");
                Bitmap image = DataManager.GetImage(code);
                if (generateLarge)
                    image.Save("./picn/" + fileName, encoderInfo, encoderParams);
                if (generateSmall)
                    DataManager.Zoom(image, 177, 254).Save("./pics/" + fileName, encoderInfo, encoderParams);
                if (generateThumb)
                    DataManager.Zoom(image, 44, 64).Save("./pics/thumbnail/" + fileName, encoderInfo, encoderParams);
                image.Dispose();
            }
        }
    }
}

