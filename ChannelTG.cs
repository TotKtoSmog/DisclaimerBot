using System.Xml.Serialization;

namespace DisclaimerBot
{
    [XmlRoot("channels")]
    public class ChannelsTG
    {
        [XmlElement("channel")]
        public List<ChannelTG> Channels;

        public ChannelsTG() =>  Channels = [];
        public ChannelsTG(List<ChannelTG> channels) => Channels = channels;
        public static ChannelsTG ChannelsAdminsInfo(ChannelsTG channels, long chat_id, List<User> users)
        {
            foreach (ChannelTG channel in channels.Channels)
                if (channel.ChatID == chat_id)
                    channel.ChatAdmins = users;
            return channels;
        }

        public static ChannelTG? GetChannel(ChannelsTG channels, long chat_id)
            => channels.Channels.Where(c => c.ChatID == chat_id).FirstOrDefault();
    }

    [XmlRoot("channel")]
    public class ChannelTG
    {
        [XmlElement("channel_name")]
        public string ChatName { get; set; }
        [XmlElement("channel_id")]
        public long ChatID { get; set; }
        [XmlElement("channel_admins")]
        public List<User> ChatAdmins { get; set; }
        [XmlElement("channel_disclaimer")]
        public string ChatDisclaimer { get; set; }
        [XmlElement("channel_disclaimer_state")]
        public bool ChatDisclaimerState { get; set; }

        public ChannelTG() 
        {
            ChatName = "";
            ChatID = long.MinValue;
            ChatAdmins = [];
            ChatDisclaimer = "";
            ChatDisclaimerState = false;
        }
        public ChannelTG(string chatName, long chatID, List<User> chatAdmins, string chatDisclaimer, bool chatDisclaimerState)
        {
            ChatName = chatName;
            ChatID = chatID;
            ChatAdmins = chatAdmins;
            ChatDisclaimer = chatDisclaimer;
            ChatDisclaimerState = chatDisclaimerState;
        }
    }
}
