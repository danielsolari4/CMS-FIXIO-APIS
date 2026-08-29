namespace Ray.Model.NewContext.Entities
{
    public partial class Page
    {
        public Page()
        {
            ViewsCount = 0;
            ShareCount = 0;
            Discriminator = "Page";
        }
    }
}
