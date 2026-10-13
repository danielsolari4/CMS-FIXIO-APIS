namespace CMS.NewsVersion.Configuration
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string NameCollectionAssetsViewCount { get; set; } = "assetviewscounts";
        public string NameCollectionNewsViewStats { get; set; } = "newsviewstats";
    }

    public class ContentSettings
    {
        public string TimeZone { get; set; } = string.Empty;
    }
}
