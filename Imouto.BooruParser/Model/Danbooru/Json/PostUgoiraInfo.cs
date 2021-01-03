using Newtonsoft.Json;

namespace Imouto.BooruParser.Model.Danbooru.Json
{
    public class PostUgoiraInfo
    {
        [JsonProperty("pixiv_ugoira_frame_data")]
        public UgoiraFrameData FrameData { get; set; }
    }
}
