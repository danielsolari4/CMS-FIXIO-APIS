

namespace Rino.Model.NewContext.Entities
{
    public partial class ProgrammingGuide : IAuditableEntity
    {
        partial void OnCreated()
        {
            IsEnabled = true;
        }
    }
}
