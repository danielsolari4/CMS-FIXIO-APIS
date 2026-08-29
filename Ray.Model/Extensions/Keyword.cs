using System;

namespace Ray.Model.NewContext.Entities
{
    public partial class Keyword
    {
        partial void OnCreated()
        {
            IsEnabled = true;
            CreationDate = DateTime.UtcNow;
        }

        public Keyword(string name)
            : this()
        {
            Name = name;
        }
    }
}
