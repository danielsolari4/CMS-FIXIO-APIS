using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class Template
    {
        public Template()
        {
            AssetContents = new HashSet<AssetContent>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }

        public virtual ICollection<AssetContent> AssetContents { get; set; }
    }
}
