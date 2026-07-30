using MMAC.DTOS;

namespace MMAC.Services.DashboardService
{
    public interface IDashboardService
    {
        Task<DashboardDTO> GetDashboardDataAsync(DateTime? fromDate, DateTime? toDate);
        Task<PagedTravellerResult> GetFilteredTravellersAsync(
            string? status, int? regionId, int? districtId, int? townshipId,
            DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    }
}