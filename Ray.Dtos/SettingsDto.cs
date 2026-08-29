using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ray.Dtos
{
    public class SettingsDto : BaseDto
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
