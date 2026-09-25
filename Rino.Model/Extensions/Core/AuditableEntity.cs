using System;

namespace Rino.Model.NewContext.Entities
{
    public abstract class AuditableEntity : IAuditableEntity
    {
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }
    }
}
