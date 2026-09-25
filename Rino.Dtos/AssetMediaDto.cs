namespace Rino.Dtos
{
    public class AssetMediaDto
    {
        public MediaDto Media { get; set; }

        public AssetDto Asset { get; set; }

        public bool Featured { get; set; }
    }
}
