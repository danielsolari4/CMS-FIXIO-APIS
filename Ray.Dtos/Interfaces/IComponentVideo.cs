namespace Ray.Dtos.Interfaces
{
    public interface IComponentVideo
    {
        int Id { get; set; }
        string Title { get; set; }
        string Url { get; set; }
        MediaSizesPaths MediaSizesPaths { get; set; }
        string MediaUrl { get; set; }

        void Parse(MediaDto asset, string imageUrl);
    }
}