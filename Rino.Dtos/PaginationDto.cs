namespace Rino.Dtos
{
    public class PaginationDto
    {
        public int? PageNumber { get; set; }

        public int? PageSize { get; set; }

        public int? Total { get; set; }

        public string Previous { get; set; }

        public string Next { get; set; }

        public int? Skip()
        {
            return PageNumber != null && PageNumber > 0 && PageSize != null ? (PageNumber - 1) * PageSize : (int?)null;
        }

        public int? Take()
        {
            return PageSize != null ? PageSize : (int?)null;
        }

        public PaginationDto() { }
    }
}
