using MMAC.DTOS;
using MMAC.Repositories.DashboardRepository;

namespace MMAC.Services.DashboardService
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repo;

        public DashboardService(IDashboardRepository repo)
        {
            _repo = repo;
        }

        public async Task<DashboardDTO> GetDashboardDataAsync(DateTime? fromDate, DateTime? toDate)
        {
            var apps = fromDate.HasValue && toDate.HasValue
                ? await _repo.GetApplicationsAsync(fromDate.Value, toDate.Value)
                : await _repo.GetAllApplicationsAsync();

            var today = DateTime.UtcNow.Date;

            // Build monthly counts for the current year
            var monthlyData = Enumerable.Range(1, 12).Select(m => new MonthlyStatusCount
            {
                Month = m,
                Submitted = apps.Count(a => a.CreatedDate.Month == m && a.CreatedDate.Year == today.Year && a.AppStatus == "Submitted"),
                Invalid = apps.Count(a => a.CreatedDate.Month == m && a.CreatedDate.Year == today.Year && a.AppStatus == "Invalid"),
                Expired = apps.Count(a => a.CreatedDate.Month == m && a.CreatedDate.Year == today.Year && a.AppStatus == "Expired"),
                Arrived = apps.Count(a => a.CreatedDate.Month == m && a.CreatedDate.Year == today.Year && a.AppStatus == "Arrived")
            }).ToList();

            // Top 10 nationalities
            var totalCount = apps.Count;
            var topNationalities = apps
                .Where(a => a.Traveller != null)
                .GroupBy(a => new
                {
                    a.Traveller!.NationalityCode,
                    Name = a.Traveller.Nationality?.Name ?? a.Traveller.NationalityCode
                })
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new NationalityCount
                {
                    NationalityCode = g.Key.NationalityCode,
                    NationalityName = g.Key.Name,
                    Count = g.Count(),
                    Percentage = totalCount > 0
                        ? Math.Round((double)g.Count() / totalCount * 100, 2)
                        : 0
                })
                .ToList();

            return new DashboardDTO
            {
                SubmittedApplicationCount = apps.Count(a => a.AppStatus == "Submitted"),
                InvalidApplicationCount = apps.Count(a => a.AppStatus == "Invalid"),
                ExpiredApplicationCount = apps.Count(a => a.AppStatus == "Expired"),
                ArrivedApplicationCount = apps.Count(a => a.AppStatus == "Arrived"),
                TotalApplicationCount = apps.Count,


                TodayTotalCount = apps.Count(a => a.CreatedDate.Date == today),
                TodaySubmittedCount = apps.Count(a => a.CreatedDate.Date == today && a.AppStatus == "Submitted"),
                TodayInvalidCount = apps.Count(a => a.CreatedDate.Date == today && a.AppStatus == "Invalid"),
                TodayExpiredCount = apps.Count(a => a.CreatedDate.Date == today && a.AppStatus == "Expired"),
                TodayArrivedCount = apps.Count(a => a.CreatedDate.Date == today && a.AppStatus == "Arrived"),

                MonthlyStatusCounts = monthlyData,
                TopNationalities = topNationalities
            };
        }

        //public async Task<DashboardDTO> GetDashboardDataAsync(DateTime? fromDate, DateTime? toDate)
        //{
        //    var apps = fromDate.HasValue && toDate.HasValue
        //        ? await _repo.GetApplicationsAsync(fromDate.Value, toDate.Value)
        //        : await _repo.GetAllApplicationsAsync();

        //    var today = DateTime.UtcNow.Date;

        //    // Build monthly counts for the current year
        //    var monthlyData = Enumerable.Range(1, 12).Select(m => new MonthlyStatusCount
        //    {
        //        Month = m,
        //        Submitted = apps.Count(a => a.CreatedDate.Month == m && a.CreatedDate.Year == today.Year && a.AppStatus == "Submitted"),
        //        Invalid = apps.Count(a => a.CreatedDate.Month == m && a.CreatedDate.Year == today.Year && a.AppStatus == "Invalid"),
        //        Expired = apps.Count(a => a.CreatedDate.Month == m && a.CreatedDate.Year == today.Year && a.AppStatus == "Expired")
        //    }).ToList();

        //    // Top 10 nationalities
        //    var totalCount = apps.Count;
        //    var topNationalities = apps
        //        .Where(a => a.Traveller != null)
        //        .GroupBy(a => new
        //        {
        //            a.Traveller!.NationalityCode,
        //            Name = a.Traveller.Nationality?.Name ?? a.Traveller.NationalityCode
        //        })
        //        .OrderByDescending(g => g.Count())
        //        .Take(10)
        //        .Select(g => new NationalityCount
        //        {
        //            NationalityCode = g.Key.NationalityCode,
        //            NationalityName = g.Key.Name,
        //            Count = g.Count(),
        //            Percentage = totalCount > 0
        //                ? Math.Round((double)g.Count() / totalCount * 100, 2)
        //                : 0
        //        })
        //        .ToList();

        //    return new DashboardDTO
        //    {
        //        SubmittedApplicationCount = apps.Count(a => a.AppStatus == "Submitted"),
        //        InvalidApplicationCount = apps.Count(a => a.AppStatus == "Invalid"),
        //        ExpiredApplicationCount = apps.Count(a => a.AppStatus == "Expired"),
        //        TotalApplicationCount = apps.Count,

        //        TodayTotalCount = apps.Count(a => a.CreatedDate == today),
        //        TodaySubmittedCount = apps.Count(a => a.CreatedDate == today && a.AppStatus == "Submitted"),
        //        TodayInvalidCount = apps.Count(a => a.CreatedDate == today && a.AppStatus == "Invalid"),
        //        TodayExpiredCount = apps.Count(a => a.CreatedDate == today && a.AppStatus == "Expired"),

        //        MonthlyStatusCounts = monthlyData,
        //        TopNationalities = topNationalities
        //    };
        //}

        public async Task<PagedTravellerResult> GetFilteredTravellersAsync(
            string? status, int? regionId, int? districtId, int? townshipId,
            DateTime? fromDate, DateTime? toDate, int page, int pageSize)
        {
            return await _repo.GetFilteredTravellersAsync(
                status, regionId, districtId, townshipId, fromDate, toDate, page, pageSize);
        }
    }
}