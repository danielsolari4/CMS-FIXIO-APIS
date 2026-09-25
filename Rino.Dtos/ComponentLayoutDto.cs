using System.ComponentModel.DataAnnotations;

namespace Rino.Dtos
{
    public class ComponentLayoutDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "ComponentLayoutId")]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Html { get; set; }

        //public int ComponentId { get; set; }
    }
}
