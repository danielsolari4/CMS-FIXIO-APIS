using System;

namespace Ray.Dtos.Interfaces
{
    public interface IComponentNew
    {
        int Id { get; set; }
        int[] NewsId { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        string Url { get; set; }
        MediaSizesPaths MediaSizesPaths { get; set; }
        string[] AuthorName { get; set; }
        DateTime LastModificationDate { get; set; }
        string PublicationDate { get; set; }
        string NodeContentTitle { get; set; }
        string MediaUrl { get; set; }
        string MediaDiscriminator { get; set; }

        void Parse(AssetSolrDto asset, string imageUrl);
    }
}