using System;
using System.Collections.Generic;

namespace Rino.Dtos
{
    public class NewsViewStatsDashboardDto
    {
        public NewsViewStatsSummaryDto Summary { get; set; } = new NewsViewStatsSummaryDto();
        public List<NewsViewStatsDashboardItemDto> RecentNotes { get; set; } = new List<NewsViewStatsDashboardItemDto>();
        public List<NewsViewStatsDashboardItemDto> TopToday { get; set; } = new List<NewsViewStatsDashboardItemDto>();
        public List<NewsViewStatsDashboardItemDto> TopMonth { get; set; } = new List<NewsViewStatsDashboardItemDto>();
        public NewsViewStatsDashboardItemDto Highlight { get; set; }
    }

    public class NewsViewStatsSummaryDto
    {
        public int NewsWithStats { get; set; }
        public int NewsWithoutStats { get; set; }
        public long TotalViews { get; set; }
        public long TodayViews { get; set; }
        public long MonthViews { get; set; }
    }

    public class NewsViewStatsDashboardItemDto
    {
        public int NewsId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Section { get; set; }
        public string Status { get; set; }
        public DateTime? PublicationDate { get; set; }
        public DateTime? LastViewAt { get; set; }
        public string TodayDate { get; set; }
        public string CurrentMonth { get; set; }
        public bool HasStats { get; set; }
        public long TotalViews { get; set; }
        public long TodayViews { get; set; }
        public long MonthViews { get; set; }
    }
}
