

namespace Rino.Model.NewContext.Entities
{
    public partial class Media : IAuditableEntity
    {
        partial void OnCreated()
        {
            Version = 1;
            ViewsCount = 0;
            ShareCount = 0;
        }
    }
}
