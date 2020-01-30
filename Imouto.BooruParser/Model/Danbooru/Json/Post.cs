namespace Imouto.BooruParser.Model.Danbooru.Json
{
    public class Post
    {
        public int id { get; set; }

        public string created_at { get; set; }

        public int uploader_id { get; set; }

        public int score { get; set; }

        public string source { get; set; }

        public string md5 { get; set; }

        public object last_comment_bumped_at { get; set; }

        public string rating { get; set; }

        public int image_width { get; set; }

        public int image_height { get; set; }

        public string tag_string { get; set; }

        public bool is_note_locked { get; set; }

        public int fav_count { get; set; }

        public string file_ext { get; set; }

        public object last_noted_at { get; set; }

        public bool is_rating_locked { get; set; }

        public object parent_id { get; set; }

        public bool has_children { get; set; }

        public int? approver_id { get; set; }

        public int tag_count_general { get; set; }

        public int tag_count_artist { get; set; }

        public int tag_count_character { get; set; }

        public int tag_count_copyright { get; set; }

        public int file_size { get; set; }

        public bool is_status_locked { get; set; }

        public string fav_string { get; set; }

        public string pool_string { get; set; }

        public int up_score { get; set; }

        public int down_score { get; set; }

        public bool is_pending { get; set; }

        public bool is_flagged { get; set; }

        public bool is_deleted { get; set; }

        public int tag_count { get; set; }

        public string updated_at { get; set; }

        public bool is_banned { get; set; }

        public int? pixiv_id { get; set; }

        public object last_commented_at { get; set; }

        public bool has_active_children { get; set; }

        public int bit_flags { get; set; }

        public string uploader_name { get; set; }

        public bool? has_large { get; set; }

        public string tag_string_artist { get; set; }

        public string tag_string_character { get; set; }

        public string tag_string_copyright { get; set; }

        public string tag_string_general { get; set; }

        public bool has_visible_children { get; set; }

        public string file_url { get; set; }

        public string large_file_url { get; set; }

        public string preview_file_url { get; set; }
    }
}
