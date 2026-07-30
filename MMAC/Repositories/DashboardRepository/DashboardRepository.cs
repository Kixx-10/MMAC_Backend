using Microsoft.EntityFrameworkCore;
using MMAC.Data;
using MMAC.DTOS;
using MMAC.Models.Cores;

namespace MMAC.Repositories.DashboardRepository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ArrivalApplication>> GetApplicationsAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.ArrivalApplication
                .Include(a => a.Traveller)
                    .ThenInclude(t => t!.Nationality)
                .Include(a => a.Township)
                    .ThenInclude(t => t!.District)
                        .ThenInclude(d => d!.StateRegion)
                .Where(a => a.CreatedDate >= fromDate && a.CreatedDate <= toDate)
                .ToListAsync();
        }

        public async Task<List<ArrivalApplication>> GetAllApplicationsAsync()
        {
            return await _context.ArrivalApplication
                .Include(a => a.Traveller)
                    .ThenInclude(t => t!.Nationality)
                .Include(a => a.Township)
                    .ThenInclude(t => t!.District)
                        .ThenInclude(d => d!.StateRegion)
                .ToListAsync();
        }

        public async Task<PagedTravellerResult> GetFilteredTravellersAsync(
            string? status, int? regionId, int? districtId, int? townshipId,
            DateTime? fromDate, DateTime? toDate, int page, int pageSize)
        {
            var query = _context.ArrivalApplication
                .Include(a => a.Traveller)
                    .ThenInclude(t => t!.Nationality)
                .Include(a => a.Township)
                    .ThenInclude(t => t!.District)
                        .ThenInclude(d => d!.StateRegion)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(a => a.AppStatus == status);

            if (townshipId.HasValue)
                query = query.Where(a => a.TownshipId == townshipId.Value);
            else if (districtId.HasValue)
                query = query.Where(a => a.DistrictId == districtId.Value);
            else if (regionId.HasValue)
                query = query.Where(a => a.StateRegionId == regionId.Value);

            if (fromDate.HasValue)
                query = query.Where(a => a.CreatedDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(a => a.CreatedDate <= toDate.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new TravellerListDTO
                {
                    AppNo = a.AppNo.ToString(),
                    ReferenceNo = a.ReferenceNo,
                    ReportDate = a.CreatedDate,
                    AppStatus = a.AppStatus ?? string.Empty,
                    FullName = a.Traveller != null ? a.Traveller.FullName : string.Empty,
                    PassportNo = a.Traveller != null ? a.Traveller.PassportNo : string.Empty,
                    ArrivalDate = a.ArrivalDate,
                    Accommodation = a.Accommodation,
                    NationalityCode = a.Traveller != null ? a.Traveller.NationalityCode : null,
                    NationalityName = a.Traveller != null && a.Traveller.Nationality != null
                        ? a.Traveller.Nationality.Name : null
                })
                .ToListAsync();

            return new PagedTravellerResult
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}