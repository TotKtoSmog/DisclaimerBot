using System.Xml.Serialization;

namespace DisclaimerBot
{
    [XmlRoot("сhannels")]
    public class СhannelsTG
    {
        [XmlElement("сhannel")]
        public List<СhannelTG> Channels;

        public СhannelsTG()
        {
            Channels = new List<СhannelTG> ();
        }
        public СhannelsTG(List<СhannelTG> channels)
        {
            Channels = channels;
        }
    }

    [XmlRoot("сhannel")]
    public class СhannelTG
    {
        [XmlElement("сhannel_name")]
        public string ChatName { get; set; }
        [XmlElement("сhannel_id")]
        public long ChatID { get; set; }
        [XmlElement("channe_admins")]
        public List<User> ChatAdmins { get; set; }
        [XmlElement("channe_disclaimer")]
        public string ChatDisclaimer { get; set; }
        [XmlElement("channe_disclaimer_state")]
        public bool ChatDisclaimerState { get; set; }

        public СhannelTG() { }
        public СhannelTG(string chatlName, long chatID, List<User> chatAdmins, string chatDisclaimer, bool chatDisclaimerState)
        {
            ChatName = chatlName;
            ChatID = chatID;
            ChatAdmins = chatAdmins;
            ChatDisclaimer = chatDisclaimer;
            ChatDisclaimerState = chatDisclaimerState;
        }
    }
}
