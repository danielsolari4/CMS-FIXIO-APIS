using System.Collections.Generic;

namespace Ray.Dtos
{
    public class PrintEditionDto : BaseDto
    {
        public string Edition { get; set; }
        public int NodeId { get; set; }
        public string Structure { get; set; }
        public string Identifier { get; set; }
        public System.DateTime MigrationDate { get; set; }

        public NodeDto Node { get; set; }
        public bool IsPublished { get; set; }
        public ICollection<PrintEditionNewDto> PrintEditionNew { get; set; }
    }

    public class ImagesPrintEiditon
    {
        public string path { get; set; }
        public int page { get; set; }
    }

    public class PrintEditionNewDto : BaseDto
    {
        public int PrintEditionId { get; set; }
        public int NewId { get; set; }
        public string Page { get; set; }

        public NewsDto News { get; set; }
        public PrintEditionDto PrintEdition { get; set; }
    }
}