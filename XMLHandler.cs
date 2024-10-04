using System.Xml.Serialization;

namespace DisclaimerBot
{
    internal static class XMLHandler
    {
        public static string path = "data.xml";
        public static ChannelsTG ReadXML()
        {
            ChannelsTG channelInfo = new();
            if (System.IO.File.Exists(path))
            {
                XmlSerializer formatter = new(typeof(ChannelsTG));
                
                using (FileStream fs = new(path, FileMode.OpenOrCreate))
                {
                    channelInfo = formatter.Deserialize(fs) as ChannelsTG;
                }

            }
            return channelInfo;
        }
        public static void WriteXML(ChannelTG info)
        {
            ChannelsTG temp = new();

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

            XmlSerializer serialize = new(typeof(ChannelsTG));
            using (FileStream fs = new(path, FileMode.Create))
                serialize.Serialize(fs, temp);
            Console.WriteLine("Объект записан в XML-документ.");
        }

        public static void WriteXML(ChannelsTG info)
        {
            XmlSerializer serialize = new(typeof(ChannelsTG));
            using (FileStream fs = new(path, FileMode.Create))
                serialize.Serialize(fs, info);
            Console.WriteLine("Объект записан в XML-документ.");
        }
    }
}
