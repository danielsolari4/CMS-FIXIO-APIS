namespace Ray.Dtos
{
    public class AssetJsonDto : AssetJsonProperties, IAssetJsonRelation
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
    }

    public class AssetJsonProperties
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Content { get; set; }
        public string ItemMenu { get; set; }
    }

    public interface IAssetJsonRelation
    {
        int Id { get; set; }
        int AssetId { get; set; }
    }
}
