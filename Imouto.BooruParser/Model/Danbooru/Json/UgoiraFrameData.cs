using System.Collections.Generic;
using Newtonsoft.Json;

namespace Imouto.BooruParser.Model.Danbooru.Json
{
    public class UgoiraFrameData
    {
        [JsonProperty("data")]
        public IReadOnlyCollection<FrameData> Data { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }
    }
}