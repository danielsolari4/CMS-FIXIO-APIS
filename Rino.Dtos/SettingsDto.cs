using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rino.Dtos
{
    public class SettingsDto : BaseDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
