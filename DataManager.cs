namespace ImgGen
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.IO;
    using System.Text.RegularExpressions;

    public class DataManager
    {
        private static Dictionary<int, Data> cardDatas = new Dictionary<int, Data>();

        private static SQLiteConnection conn;
        private static object locker = new object();

        public static void InitialDatas(string dbPath)
        {
            conn = new SQLiteConnection("Data Source=" + dbPath);
            conn.Open();
            SQLiteCommand command = new SQLiteCommand(conn);
            command.CommandText = "SELECT * FROM datas JOIN texts on datas.id = texts.id";
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Data data = new Data();
                data.code = (int)(long)reader["id"];
                data.alias = (int)(long)reader["alias"];
                data.setcode = (int)(long)reader["setcode"];
                data.type = (Type)(long)reader["type"];
                data.attack = (int)(long)reader["atk"];
                data.defence = (int)(long)reader["def"];
                data.level = (int)(long)reader["level"];
                data.race = (Race)(long)reader["race"];
                data.attribute = (Attribute)(long)reader["attribute"];
                data.name = (string)reader["name"];
                data.text = (string)reader["desc"];
                cardDatas.Add(data.code, data);
            }
            reader.Close();
            command.Dispose();
            conn.Close();
        }

        public static Data GetData(int code)
        {
            if (cardDatas.ContainsKey(code))
            {
                return cardDatas[code];
            }
            else
            {
                Console.WriteLine($"Card {code} not found!");
                Data data = new Data
                {
                    code = 0,
                    name = "???",
                    text = "???"
                };
                return data;
            }
        }

        public static string FormatCardDesc(string r)
        {
            char[] chArray = r.ToCharArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                if ((chArray[i] > ' ') && (chArray[i] < '\x007f')) // 半角转全角
                {
                    chArray[i] = (char)(chArray[i] + 0xfee0);
                }
                if (chArray[i] == '\x00b7') // another middle dot
                {
                    chArray[i] = '・';
                }
            }
            string desc = new string(chArray);
            desc = desc.Replace(Environment.NewLine, "\n");
            desc = Regex.Replace(desc, @"(（注：.*[\n\s]+)", ""); // 去掉注释
            desc = Regex.Replace(desc, @"(?<=。)([\n\s]+)(?=[①②③④⑤⑥⑦⑧⑨⑩])", ""); // 去掉效果编号前的换行
            desc = Regex.Replace(desc, @"([\n\s]+)(?=●)(?=.+。)", ""); // 去掉●号前的换行
            return desc;
        }

        public static string FormatCardName(string r)
        {
            return r.Replace('\x00b7', '・'); // another middle dot
        }
    }
}
