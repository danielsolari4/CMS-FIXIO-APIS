using System.Collections.Generic;

namespace Rino.Dtos
{
    public class PageContentDto : AssetContentDto
    {
        public string Keywords { get; set; }

        public string Content { get; set; }

        public string MetaAuthor { get; set; }

        public string MetaDescription { get; set; }

        public string MetaKeywords { get; set; }

        public string BackgroundColor { get; set; }

        public List<KeywordDto> KeywordList { get; set; }

        public List<SocialNetworkDto> SocialNetwork { get; set; }

        public PageContentDto()
        {
            KeywordList = new List<KeywordDto>();
            SocialNetwork = new List<SocialNetworkDto>();
        }
    }
}
