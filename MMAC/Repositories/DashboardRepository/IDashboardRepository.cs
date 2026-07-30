using MMAC.DTOS;
using MMAC.Models.Cores;

namespace MMAC.Repositories.DashboardRepository
{
    public interface IDashboardRepository
    {
        Task<List<ArrivalApplication>> GetApplicationsAsync(DateTime fromDate, DateTime toDate);
        Task<List<ArrivalApplication>> GetAllApplicationsAsync();
        Task<PagedTravellerResult> GetFilteredTravellersAsync(
            string? status, int? regionId, int? districtId, int? townshipId,
            DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    }
}