using System.ComponentModel.DataAnnotations;

namespace Ray.Dtos
{
    public class CountDto
    {
        public virtual int Id { get; set; }

        public virtual int Count { get; set; }

        public CountDiscriminator Discriminator { get; set; }
    }

    public class UpdateCountDto : CountDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Count")]
        public override int Count { get; set; }
    }

    public enum CountDiscriminator
    {
        Share,
        Views
    }
}