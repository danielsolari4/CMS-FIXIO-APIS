using System;

namespace Ray.Dtos
{
    public abstract class BaseDto
    {
        public virtual int Id { get; set; }

        public string CreationUser { get; set; }

        public DateTime CreationDate { get; set; }

        public string LastModificationUser { get; set; }

        public DateTime? LastModificationDate { get; set; }
    }
}
