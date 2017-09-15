namespace Imouto.BooruParser.Model
{
    public class SettingsM
    {
        public string SankakuLogin { get; set; }

        public string SankakuPassHash { get; set; }

        public int SankakuLoadDelay { get; set; }

        public bool IsSankakuActive => !string.IsNullOrWhiteSpace(SankakuLogin) && !string.IsNullOrWhiteSpace(SankakuPassHash);

        public string DanbooruLogin { get; set; }

        public string DanbooruApiKey { get; set; }

        public int DanbooruLoadDelay { get; set; }

        public void Initialize()
        {
            SankakuLoadDelay = 4761;
            DanbooruLoadDelay = 7200;
        }
    }
}
