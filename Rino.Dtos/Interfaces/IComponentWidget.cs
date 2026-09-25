namespace Rino.Dtos.Interfaces
{
    public interface IComponentWidget
    {
        int Id { get; set; }
        string Name { get; set; }
        string Html { get; set; }
        int WidgetTypeId { get; set; }

        void Parse(WidgetDto asset);
    }
}