namespace SavyWeb.Classes
{
    public class SavyDBClass
    {
        public int asset_img_id { get; set; }
        public string asset_img_name { get; set; } = string.Empty;
        public string asset_img_tag { get; set; } = string.Empty;
        public string asset_img_url { get; set; } = string.Empty;
        public int deleted { get; set; }
        public string rowcode { get; set; } = string.Empty;
        public string created_by { get; set; } = string.Empty;
        public string updated_by { get; set; } = string.Empty;
        public DateTime ts_createt { get; set; }
        public DateTime ts_update { get; set; }
        public DateTime ts_delete { get; set; }

        public SavyDBClass()
        {
            ts_createt = DateTime.Now;
            ts_update = DateTime.Now;
        }
    }
}
