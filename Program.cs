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
            EncoderParameter parameter = new EncoderParameter(quality, 0x5fL);
            encoderParams.Param[0] = parameter;
            string[] files = Directory.GetFiles("./pico", "*.jpg");
            Directory.CreateDirectory("./picn/thumbnail");
            foreach (string str in files)
            {
                int code = int.Parse(Path.GetFileNameWithoutExtension(str));
                string fileName = Path.GetFileName(str);
                Console.WriteLine("Generating {0}", fileName);
                Bitmap image = DataManager.GetImage(code);
                image.Save("./picn/" + fileName, encoderInfo, encoderParams);

                DataManager.Zoom(image, 44, 64).Save("./picn/thumbnail/" + fileName, encoderInfo, encoderParams);
                /*
                Bitmap thumbnail = new Bitmap(44, 64);
                Graphics graph = Graphics.FromImage(thumbnail);
                graph.DrawImage(image, 0, 0, 44, 64);
                thumbnail.Save("./picn/thumbnail/" + fileName, encoderInfo, encoderParams);
                thumbnail.Dispose();
                 */
                /*
                Bitmap thumbnail = new Bitmap(image, 44, 64);
                thumbnail.Save("./picn/thumbnail/" + fileName, encoderInfo, encoderParams);
                thumbnail.Dispose();
                 */
                image.Dispose();
            }
        }
    }
}

