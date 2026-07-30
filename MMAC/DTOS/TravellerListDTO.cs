namespace MMAC.DTOS
{
    public class TravellerListDTO
    {
        public string AppNo { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public string AppStatus { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PassportNo { get; set; } = string.Empty;
        public DateTime ArrivalDate { get; set; }
        public string? Accommodation { get; set; }
        public string? NationalityCode { get; set; }
        public string? NationalityName { get; set; }
    }

    public class PagedTravellerResult
    {
        public List<TravellerListDTO> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
