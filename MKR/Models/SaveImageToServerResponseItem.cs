namespace MKR.Models
{
    public class SaveImageToServerResponseItem
    {
        public int album_id { get; set; }
        public int date { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public ICollection<SaveImageToServerResponseItemSize> sizes { get; set; }
        public string text { get; set; }
        public int user_id { get; set; }
        public bool has_tags { get; set; }       
    }
}
