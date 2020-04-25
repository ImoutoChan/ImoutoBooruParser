using System;
using Newtonsoft.Json;

namespace Imouto.BooruParser.Model.Danbooru.Json
{
    public class PostVersion
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("post_id")]
        public int PostId { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}