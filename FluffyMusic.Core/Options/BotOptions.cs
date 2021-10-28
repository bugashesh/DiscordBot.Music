namespace FluffyMusic.Core.Options
{
    public class BotOptions
    {
        public string Token { get; set; }
        public string Prefix { get; set; } = "/";
        public string[] Activities { get; set; }
        public int SwitchActivityInterval { get; set; }

        //  In seconds
        public int RemoveNotificationDelay { get; set; }
    }
}
