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

        public static void InitialDatas(string dbPath)
        {
            conn = new SQLiteConnection("Data Source=" + dbPath);
            conn.Open();
            SQLiteCommand command = new SQLiteCommand(conn)
            {
                CommandText = "SELECT * FROM datas JOIN texts ON datas.id = texts.id"
            };
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Data data = new Data
                {
                    code = (int)(long)reader["id"],
                    alias = (int)(long)reader["alias"],
                    setcode = (int)(long)reader["setcode"],
                    type = (Type)(long)reader["type"],
                    attack = (int)(long)reader["atk"],
                    defence = (int)(long)reader["def"],
                    level = (int)(long)reader["level"],
                    race = (Race)(long)reader["race"],
                    attribute = (Attribute)(long)reader["attribute"],
                    name = (string)reader["name"],
                    text = (string)reader["desc"]
                };
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
                    code = code,
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
            desc = Regex.Replace(desc, @"(?<=。)([\n\s]+)(?=[①②③④⑤⑥⑦⑧⑨⑩])", ""); // 去掉效果编号前的换行
            desc = Regex.Replace(desc, @"([\n\s]+)(?=●)", ""); // 去掉●号前的换行
            return desc;
        }

        public static string FormatCardName(string r)
        {
            return r.Replace('\x00b7', '・'); // another middle dot
        }
    }
}

