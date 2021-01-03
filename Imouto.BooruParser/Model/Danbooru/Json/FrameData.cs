using Newtonsoft.Json;

namespace Imouto.BooruParser.Model.Danbooru.Json
{
    public class FrameData
    {
        [JsonProperty("delay")]
        public int Delay { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }
    }
}