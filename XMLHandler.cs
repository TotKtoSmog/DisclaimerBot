using System.Xml.Serialization;

namespace DisclaimerBot
{
    internal static class XMLHandler
    {
        public static string path = "data.xml";
        public static СhannelsTG ReadXML()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(СhannelsTG));
            СhannelsTG channelInfo;
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                channelInfo = formatter.Deserialize(fs) as СhannelsTG;
            }
            return channelInfo;
        }
        public static void WriteXML(СhannelTG info)
        {
            СhannelsTG temp = new СhannelsTG();

            if (System.IO.File.Exists(path))
            {
                temp = ReadXML();

                bool flag = false;

                for (int i = 0; i < temp.Channels.Count; i++)
                {
                    if (temp.Channels[i].ChatID == info.ChatID)
                    {
                        temp.Channels[i] = info;
                        flag = true;
                        break;
                    }
                }
                if (!flag) temp.Channels.Add(info);
            }
            else
            {
                temp.Channels.Add(info);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(СhannelsTG));
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                serializer.Serialize(fs, temp);
            Console.WriteLine("Объект записан в XML-документ.");
        }
    }
}
