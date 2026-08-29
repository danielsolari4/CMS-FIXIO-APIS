

namespace Ray.Model.NewContext.Entities
{
    public partial class AssetContent : IAuditableEntity
    {
        public AssetContent()
        {
            Discriminator = "Page";
        }
    }
}
