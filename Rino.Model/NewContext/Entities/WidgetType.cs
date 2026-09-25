using System;
using System.Collections.Generic;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class WidgetType
    {
        public WidgetType()
        {
            Widgets = new HashSet<Widget>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }

        public virtual ICollection<Widget> Widgets { get; set; }
    }
}
