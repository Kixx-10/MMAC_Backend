namespace MMAC.DTOS
{
    public class DashboardDTO
    {
        // Summary counts (all time or filtered by date)
        public int SubmittedApplicationCount { get; set; }
        public int InvalidApplicationCount { get; set; }
        public int ExpiredApplicationCount { get; set; }
        public int TotalApplicationCount { get; set; }

        // Today's counts
        public int TodayTotalCount { get; set; }
        public int TodaySubmittedCount { get; set; }
        public int TodayInvalidCount { get; set; }
        public int TodayExpiredCount { get; set; }

        // Monthly breakdown (12 months, Month 1=Jan)
        public List<MonthlyStatusCount> MonthlyStatusCounts { get; set; } = new();

        // Top 10 nationalities
        public List<NationalityCount> TopNationalities { get; set; } = new();
    }

    public class MonthlyStatusCount
    {
        public int Month { get; set; }  // 1-12
        public int Submitted { get; set; }
        public int Invalid { get; set; }
        public int Expired { get; set; }
    }

    public class NationalityCount
    {
        public string NationalityCode { get; set; } = string.Empty;
        public string NationalityName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
