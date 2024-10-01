using System.Xml.Serialization;

namespace DisclaimerBot
{
    [XmlRoot("user")]
    public class User
    {
        [XmlElement("user_id")]
        public long UserId { get; set; }
        [XmlElement ("tg_name")]
        public string TgName { get; set; }
        public User() 
        {
            UserId = 0;
            TgName = "";
        }
        public User(long userId, string tgName)
        {
            UserId = userId;
            TgName = tgName;
        }
    }
}
