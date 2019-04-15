namespace sIRC
{
    public enum ResponseID
    {
        //TODO: ADD MORE

        RPL_WELCOME = 1, //Checked
        RPL_YOURHOST = 2, //Checked
        RPL_CREATED = 3, //Checked
        RPL_MYINFO = 4, //Checked
        RPL_BOUNCE = 5, //Checked

        RPL_UMODEIS = 221,

        RPL_LUSERCLIENT = 251,
        RPL_LUSEROP = 252,
        RPL_LUSERUNKNOWN = 253,
        RPL_LUSERCHANNELS = 254,
        RPL_LUSERME = 255,

        RPL_USERHOST = 302, //Checked
        RPL_ISON = 303, //???

        RPL_AWAY = 301, //Checked
        RPL_UNAWAY = 304, //Checked
        RPL_NOWAWAY = 305, //Checked

        RPL_WHOISUSER = 311,
        RPL_WHOISSERVER = 312,
        RPL_WHOISOPERATOR = 313,
        RPL_WHOISIDLE = 317,
        RPL_ENDOFWHOIS = 318,
        RPL_WHOISCHANNELS = 319,

        RPL_CHANNELMODEIS = 324,

        RPL_NOTOPIC = 331, //Checked
        RPL_TOPIC = 332, //Checked
        RPL_TOPICSET = 333, //Checked

        RPL_NAMREPLY = 353, //Checked
        RPL_ENDOFNAMES = 366, //Checked

        RPL_MOTD = 372, //Checked
        RPL_MOTDSTART = 375, //Checked
        RPL_ENDOFMOTD = 376, //Checked
    }
}
