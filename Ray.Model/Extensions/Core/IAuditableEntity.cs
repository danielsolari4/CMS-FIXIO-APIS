using System;

namespace Ray.Model.NewContext.Entities
{
    public interface IAuditableEntity 
    {
        string CreationUser { get; set; }
        System.DateTime CreationDate { get; set; }
        string LastModificationUser { get; set; }
        DateTime? LastModificationDate { get; set; }
    }
}
