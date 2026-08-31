using System.ComponentModel.DataAnnotations;

namespace CMS.NewsVersion.Model
{
    public class News
    {
        [Key]
        public int Id { get; set; }
        public int NewsId { get; set; }
        public string Title { get; set; }

        public string Content { get; set; }

        public string Status { get; set; }
        public DateTime LastModificationDate { get; set; }
        public string LastModificationUser { get; set; }

        public bool CacheSolr { get; set; }
    }
}
