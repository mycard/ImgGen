namespace ImgGen
{
    using System;
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
            if (args.Length > 1)
            {
                DataManager.InitialDatas(args[0], args[1]);
            }
            else if (args.Length > 0)
            {
                DataManager.InitialDatas(args[0]);
            }
            else
            {
                DataManager.InitialDatas();
            }
            Encoder quality = Encoder.Quality;
            ImageCodecInfo encoderInfo = GetEncoderInfo("image/jpeg");
            EncoderParameters encoderParams = new EncoderParameters(1);
            EncoderParameter parameter = new EncoderParameter(quality, 95L);
            encoderParams.Param[0] = parameter;
            string[] files = Directory.GetFiles("./pico", "*.jpg");
            Directory.CreateDirectory("./picn");
            Directory.CreateDirectory("./pics/thumbnail");
            foreach (string str in files)
            {
                int code = int.Parse(Path.GetFileNameWithoutExtension(str));
                string fileName = Path.GetFileName(str);
                Console.WriteLine("Generating {0}", fileName);
                Bitmap image = DataManager.GetImage(code);
                image.Save("./picn/" + fileName, encoderInfo, encoderParams);
                DataManager.Zoom(image, 177, 254).Save("./pics/" + fileName, encoderInfo, encoderParams);
                DataManager.Zoom(image, 44, 64).Save("./pics/thumbnail/" + fileName, encoderInfo, encoderParams);
                image.Dispose();
            }
        }
    }
}

