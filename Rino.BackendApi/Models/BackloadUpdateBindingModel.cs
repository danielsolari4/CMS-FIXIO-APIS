using System.ComponentModel.DataAnnotations;

namespace Rino.BackendApi.Models
{
    public class BackloadUpdateBindingModel
    {
        [Required]
        public int MediaId { get; set; }

        [Required]
        public int Width { get; set; }

        [Required]
        public int Height { get; set; }

        [Required]
        public string Base64Image { get; set; }
    }
}
