namespace Rino.Model.NewContext.Entities
{
    public partial class Asset : IAuditableEntity
    {
        partial void OnCreated()
        {
            Status = AssetStatus.DRAFT;
        }
    }

    public static class AssetStatus
    {
        public const string DRAFT = "DRAFT";
        public const string PENDING_APPROVAL = "PENDING_APPROVAL";
        public const string APPROVED = "APPROVED";
        public const string PUBLISHED = "PUBLISHED";
    }
}
