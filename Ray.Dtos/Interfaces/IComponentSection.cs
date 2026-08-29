namespace Ray.Dtos.Interfaces
{
    public interface IComponentSection
    {
        int NodeId { get; set; }
        void Parse(SectionDto section, string imageUrl);
    }
}